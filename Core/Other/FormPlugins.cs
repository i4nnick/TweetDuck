﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using TweetDck.Core.Controls;
using TweetDck.Plugins;
using TweetDck.Plugins.Controls;
using TweetDck.Plugins.Events;

namespace TweetDck.Core.Other{
    partial class FormPlugins : Form{
        private readonly PluginManager pluginManager;
        private readonly TabButton tabBtnOfficial, tabBtnCustom;
        private readonly PluginListFlowLayout flowLayoutPlugins;

        private PluginGroup? selectedGroup;

        public FormPlugins(){
            InitializeComponent();
        }

        public FormPlugins(PluginManager pluginManager) : this(){
            this.pluginManager = pluginManager;
            this.pluginManager.Reloaded += pluginManager_Reloaded;

            this.flowLayoutPlugins = new PluginListFlowLayout();
            this.flowLayoutPlugins.Resize += flowLayoutPlugins_Resize;

            this.tabPanelPlugins.SetupTabPanel(90);
            this.tabPanelPlugins.ReplaceContent(flowLayoutPlugins);

            this.tabBtnOfficial = tabPanelPlugins.AddButton("",() => SelectGroup(PluginGroup.Official));
            this.tabBtnCustom = tabPanelPlugins.AddButton("",() => SelectGroup(PluginGroup.Custom));

            this.tabPanelPlugins.SelectTab(tabBtnOfficial);
            this.pluginManager_Reloaded(pluginManager,null);

            Shown += (sender, args) => {
                Program.UserConfig.PluginsWindow.Restore(this,false);
            };

            FormClosed += (sender, args) => {
                Program.UserConfig.PluginsWindow.Save(this);
                Program.UserConfig.Save();
            };
        }

        private void SelectGroup(PluginGroup group){
            if (selectedGroup.HasValue && selectedGroup == group)return;

            selectedGroup = group;
            
            ReloadPluginTab();
        }

        public void ReloadPluginTab(){
            if (!selectedGroup.HasValue)return;

            flowLayoutPlugins.SuspendLayout();
            flowLayoutPlugins.Controls.Clear();

            foreach(Plugin plugin in pluginManager.GetPluginsByGroup(selectedGroup.Value)){
                flowLayoutPlugins.Controls.Add(new PluginControl(pluginManager,plugin));
            }

            flowLayoutPlugins_Resize(flowLayoutPlugins,new EventArgs());
            flowLayoutPlugins.ResumeLayout(true);
        }

        private void pluginManager_Reloaded(object sender, PluginLoadEventArgs e){
            tabBtnOfficial.Text = "Official: "+pluginManager.CountPluginByGroup(PluginGroup.Official);
            tabBtnCustom.Text = "Custom: "+pluginManager.CountPluginByGroup(PluginGroup.Custom);
        }

        private void flowLayoutPlugins_Resize(object sender, EventArgs e){
            int horizontalOffset = 8+(flowLayoutPlugins.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0);

            foreach(Control control in flowLayoutPlugins.Controls){
                control.Width = flowLayoutPlugins.Width-control.Margin.Horizontal-horizontalOffset;
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e){
            using(Process.Start("explorer.exe","\""+pluginManager.PathCustomPlugins+"\"")){}
        }

        private void btnReload_Click(object sender, EventArgs e){
            pluginManager.Reload();
            ReloadPluginTab();
        }

        private void btnClose_Click(object sender, EventArgs e){
            Close();
        }
    }
}