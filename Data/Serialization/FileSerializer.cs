﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace TweetDuck.Data.Serialization{
    class FileSerializer<T> where T : ISerializedObject{
        private const string NewLineReal = "\r\n";
        private const string NewLineCustom = "\r~\n";

        private static readonly ITypeConverter BasicSerializerObj = new BasicSerializer();
        
        private readonly Dictionary<string, PropertyInfo> props;
        private readonly Dictionary<Type, ITypeConverter> serializers;

        public FileSerializer(){
            this.props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.CanWrite).ToDictionary(prop => prop.Name);
            this.serializers = new Dictionary<Type, ITypeConverter>();
        }

        public void RegisterSerializer(Type type, ITypeConverter serializer){
            serializers[type] = serializer;
        }

        public void Write(string file, T obj){
            using(StreamWriter writer = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))){
                foreach(KeyValuePair<string, PropertyInfo> prop in props){
                    Type type = prop.Value.PropertyType;
                    object value = prop.Value.GetValue(obj);
                    
                    if (!serializers.TryGetValue(type, out ITypeConverter serializer)) {
                        serializer = BasicSerializerObj;
                    }

                    if (serializer.TryWriteType(type, value, out string converted)){
                        if (converted != null){
                            writer.Write($"{prop.Key} {converted.Replace(Environment.NewLine, NewLineCustom)}");
                            writer.Write(NewLineReal);
                        }
                    }
                    else{
                        throw new SerializationException($"Invalid serialization type, conversion failed for: {type}");
                    }
                }
            }
        }

        public void Read(string file, T obj){
            using(StreamReader reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))){
                foreach(string line in reader.ReadToEnd().Split(new string[]{ NewLineReal }, StringSplitOptions.RemoveEmptyEntries)){
                    int space = line.IndexOf(' ');

                    if (space == -1){
                        throw new SerializationException($"Invalid file format, missing separator: {line}");
                    }

                    string property = line.Substring(0, space);
                    string value = line.Substring(space+1).Replace(NewLineCustom, Environment.NewLine);

                    if (props.TryGetValue(property, out PropertyInfo info)){
                        if (!serializers.TryGetValue(info.PropertyType, out ITypeConverter serializer)) {
                            serializer = BasicSerializerObj;
                        }

                        if (serializer.TryReadType(info.PropertyType, value, out object converted)){
                            info.SetValue(obj, converted);
                        }
                        else{
                            throw new SerializationException($"Invalid file format, cannot convert value: {value} (property: {property})");
                        }
                    }
                    else if (!obj.OnReadUnknownProperty(property, value)){
                        throw new SerializationException($"Invalid file format, unknown property: {property}+");
                    }
                }
            }
        }

        private class BasicSerializer : ITypeConverter{
            bool ITypeConverter.TryWriteType(Type type, object value, out string converted){
                switch(Type.GetTypeCode(type)){
                    case TypeCode.Boolean:
                        converted = value.ToString();
                        return true;

                    case TypeCode.Int32:
                        converted = ((int)value).ToString(); // cast required for enums
                        return true;

                    case TypeCode.String:
                        converted = value?.ToString();
                        return true;

                    default:
                        converted = null;
                        return false;
                }
            }

            bool ITypeConverter.TryReadType(Type type, string value, out object converted){
                switch(Type.GetTypeCode(type)){
                    case TypeCode.Boolean:
                        if (bool.TryParse(value, out bool b)){
                            converted = b;
                            return true;
                        }
                        else goto default;

                    case TypeCode.Int32:
                        if (int.TryParse(value, out int i)){
                            converted = i;
                            return true;
                        }
                        else goto default;

                    case TypeCode.String:
                        converted = value;
                        return true;

                    default:
                        converted = null;
                        return false;
                }
            }
        }
    }
}