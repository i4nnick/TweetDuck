﻿using System;
using System.Windows.Forms;
using TweetDck.Configuration;
using TweetDck.Core.Handling;

namespace TweetDck.Core.Other{
    sealed partial class FormSettings : Form{
        private static UserConfig Config{
            get{
                return Program.UserConfig;
            }
        }

        private readonly FormNotification notification;
        private bool isLoaded;

        public FormSettings(FormBrowser browserForm){
            InitializeComponent();
            Shown += (sender, args) => isLoaded = true;

            Text = Program.BrandName+" Settings";

            notification = new FormNotification(browserForm,false){ CanMoveWindow = () => radioLocCustom.Checked };

            notification.Move += (sender, args) => {
                if (radioLocCustom.Checked){
                    Config.CustomNotificationPosition = notification.Location;
                }
            };

            notification.Show(this);

            switch(Config.NotificationPosition){
                case TweetNotification.Position.TopLeft: radioLocTL.Checked = true; break;
                case TweetNotification.Position.TopRight: radioLocTR.Checked = true; break;
                case TweetNotification.Position.BottomLeft: radioLocBL.Checked = true; break;
                case TweetNotification.Position.BottomRight: radioLocBR.Checked = true; break;
                case TweetNotification.Position.Custom: radioLocCustom.Checked = true; break;
            }

            switch(Config.NotificationDuration){
                case TweetNotification.Duration.Short: radioDurShort.Checked = true; break;
                case TweetNotification.Duration.Medium: radioDurMedium.Checked = true; break;
                case TweetNotification.Duration.Long: radioDurLong.Checked = true; break;
                case TweetNotification.Duration.VeryLong: radioDurVeryLong.Checked = true; break;
            }

            comboBoxDisplay.Items.Add("(Same As "+Program.BrandName+")");

            foreach(Screen screen in Screen.AllScreens){
                comboBoxDisplay.Items.Add(screen.DeviceName+" ("+screen.Bounds.Width+"x"+screen.Bounds.Height+")");
            }

            comboBoxDisplay.SelectedIndex = Math.Min(comboBoxDisplay.Items.Count-1,Config.NotificationDisplay);

            comboBoxTrayType.Items.Add("Disabled");
            comboBoxTrayType.Items.Add("Display Icon Only");
            comboBoxTrayType.Items.Add("Minimize to Tray");
            comboBoxTrayType.Items.Add("Close to Tray");
            comboBoxTrayType.SelectedIndex = Math.Min(Math.Max((int)Config.TrayBehavior,0),comboBoxTrayType.Items.Count-1);

            trackBarEdgeDistance.Value = Config.NotificationEdgeDistance;
            checkNotificationTimer.Checked = Config.DisplayNotificationTimer;
            checkUpdateNotifications.Checked = Config.EnableUpdateCheck;
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e){
            Config.Save();
        }

        private void radioLoc_CheckedChanged(object sender, EventArgs e){
            if (!isLoaded)return;

            if (radioLocTL.Checked)Config.NotificationPosition = TweetNotification.Position.TopLeft;
            else if (radioLocTR.Checked)Config.NotificationPosition = TweetNotification.Position.TopRight;
            else if (radioLocBL.Checked)Config.NotificationPosition = TweetNotification.Position.BottomLeft;
            else if (radioLocBR.Checked)Config.NotificationPosition = TweetNotification.Position.BottomRight;
            else if (radioLocCustom.Checked){
                if (!Config.IsCustomNotificationPositionSet){
                    Config.CustomNotificationPosition = notification.Location;
                }

                Config.NotificationPosition = TweetNotification.Position.Custom;
            }

            comboBoxDisplay.Enabled = trackBarEdgeDistance.Enabled = !radioLocCustom.Checked;
            notification.ShowNotificationForSettings(false);
        }

        private void radioLoc_Click(object sender, EventArgs e){
            if (!isLoaded)return;

            notification.ShowNotificationForSettings(false);
        }

        private void comboBoxDisplay_SelectedValueChanged(object sender, EventArgs e){
            if (!isLoaded)return;

            Config.NotificationDisplay = comboBoxDisplay.SelectedIndex;
            notification.ShowNotificationForSettings(false);
        }

        private void trackBarEdgeDistance_ValueChanged(object sender, EventArgs e){
            if (!isLoaded)return;

            Config.NotificationEdgeDistance = trackBarEdgeDistance.Value;
            notification.ShowNotificationForSettings(false);
        }

        private void radioDur_CheckedChanged(object sender, EventArgs e){
            if (!isLoaded)return;

            if (radioDurShort.Checked)Config.NotificationDuration = TweetNotification.Duration.Short;
            else if (radioDurMedium.Checked)Config.NotificationDuration = TweetNotification.Duration.Medium;
            else if (radioDurLong.Checked)Config.NotificationDuration = TweetNotification.Duration.Long;
            else if (radioDurVeryLong.Checked)Config.NotificationDuration = TweetNotification.Duration.VeryLong;

            notification.ShowNotificationForSettings(true);
        }

        private void radioDur_Click(object sender, EventArgs e){
            if (!isLoaded)return;

            notification.ShowNotificationForSettings(true);
        }

        private void comboBoxTrayType_SelectedIndexChanged(object sender, EventArgs e){
            if (!isLoaded)return;

            Config.TrayBehavior = (TrayIcon.Behavior)comboBoxTrayType.SelectedIndex;
        }

        private void checkNotificationTimer_CheckedChanged(object sender, EventArgs e){
            if (!isLoaded)return;

            Config.DisplayNotificationTimer = checkNotificationTimer.Checked;
            notification.ShowNotificationForSettings(true);
        }

        private void checkUpdateNotifications_CheckedChanged(object sender, EventArgs e){
            if (!isLoaded)return;

            Config.EnableUpdateCheck = checkUpdateNotifications.Checked;
        }
    }
}
