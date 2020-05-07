using AppKit;
using Foundation;
using System;
using System.Text;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;
using System.Linq;


namespace MenubarMusic
{
    public class Song
    {
        public static Song LastPlayedSong;
        public String Artist { get; set; }
        public String Title { get; set; }
        public String Album { get; set; }
        public NSImage AlbumArt { get; set; }
    }

    public class MusicPlayer
    {
        private String name;
        private String [] playlist;

        public MusicPlayer(String name)
        {
            this.name = name;

            String script = string.Format("tell application \"{0}\" to get name of user playlists", this.name);
            NSAppleScript appleScript = new NSAppleScript(script);
            NSDictionary error;
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);

            nint B = result.NumberOfItems;
            playlist = new String[B];
            for (int i = 0; i < playlist.Length; i++)
            {
                playlist[i] = result.DescriptorAtIndex(i + 1).StringValue;
            }

        }

        public String Name()
        {
            return this.name;
        }

        public void PlayPause()
        {
            String script = string.Format("tell application \"{0}\" to playpause", this.name);
            NSAppleScript appleScript = new NSAppleScript(script);
            NSDictionary error;
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);
        }

        public void AddPlaylistToMenu(NSMenu menu)
        {
            for (int i=0;i<this.playlist.Length; i++)
            {
                var playlistmenu = new NSMenuItem();
                playlistmenu.Title = this.playlist[i];
                playlistmenu.Activated += (sender, e) => {
                    NSAppleScript appleScript = new NSAppleScript("tell application \""+this.name+"\" to play playlist \""+playlistmenu.Title+"\"");
                    NSDictionary error;
                    NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);
                };
                menu.AddItem(playlistmenu);
            }
        }

        public String GetTrackName()
        {
            String script = string.Format("tell application \"{0}\" to get name of current track", this.name);
            NSAppleScript appleScript = new NSAppleScript(script);
            NSDictionary error;
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);

            if (result==null)
            {
                return "Null";
            }

            return result.StringValue;
        }
    }

    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        NSStatusItem item;
        MusicPlayer itunes;

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            //iTunes再生
            //NSAppleScript appleScript = new NSAppleScript("tell application \"iTunes\" to playpause");

            //再生中:kPSP 停止中:kPSp
            //NSAppleScript appleScript = new NSAppleScript("tell application \"iTunes\" to get player state");

            //NotificationCenterの設定
            var center = (NSDistributedNotificationCenter)NSDistributedNotificationCenter.DefaultCenter;// as NSNotificationCenter;
            center.AddObserver(new NSString("com.apple.iTunes.playerInfo"), OnClockChange);

            //ステータスバーの作成
            NSStatusBar statusBar = NSStatusBar.SystemStatusBar;
            NSMenu menu = new NSMenu("Music");

            //ステータスバーの設定
            //var item = statusBar.CreateStatusItem(NSStatusItemLength.Square);
            item = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            item.Title = "Music";
            item.HighlightMode = true;
            item.Menu = menu;

            //iTunesPlayerの追加
            itunes = new MusicPlayer("iTunes");

            var menuItem2 = new NSMenuItem();
            menuItem2.Title = "PlayPause";
            menuItem2.Activated += (sender, e) =>
            {
                itunes.PlayPause();
            };
            menu.AddItem(menuItem2);

            var menuItem = new NSMenuItem();
            menuItem.Title = "Quit";
            menuItem.Action = new ObjCRuntime.Selector("quite:");
            menu.AddItem(menuItem);

            itunes.AddPlaylistToMenu(menu);
            
            Console.OutputEncoding = Encoding.UTF8;
        }

        public void OnClockChange(NSNotification notification)
        {
            var dict = notification.UserInfo;
            dict.ToList().ForEach((item) =>
            {
                Debug.WriteLine($"{item.Key} : {item.Value}");
            });
            item.Title = dict["Name"].ToString();
            //Console.WriteLine(dict["Name"]);
        }

        [Export("quite:")]
        public void Quite(NSObject sender)
        {
            NSApplication.SharedApplication.Terminate(this);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
