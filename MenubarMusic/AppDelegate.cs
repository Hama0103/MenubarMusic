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
        public String Artist { get; set; }
        public String Title { get; set; }
        public String Album { get; set; }
        public String Player { get; set; }
    }

    public class MusicMenu
    {
        private NSStatusItem item;
        private MusicPlayer itunes;

        public MusicMenu(String _title)
        {
            //メニューの作成
            NSStatusBar statusBar = NSStatusBar.SystemStatusBar;
            NSMenu menu = new NSMenu(_title);

            this.item = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            this.item.Title = "Music";
            this.item.HighlightMode = true;
            this.item.Menu = menu;

            //itunesプレイヤー作成
            itunes = new MusicPlayer("iTunes");

            //再生停止項目追加
            var menuItem2 = new NSMenuItem();
            menuItem2.Title = "PlayPause";
            menuItem2.Activated += (sender, e) => itunes.PlayPause();
            menu.AddItem(menuItem2);

            //アプリ終了項目追加
            var menuItem = new NSMenuItem();
            menuItem.Title = "Quit";
            menuItem.Action = new ObjCRuntime.Selector("quite:");
            menu.AddItem(menuItem);

            //プレイリスト情報をメニューに追加
            var playlist = itunes.playlist;

            for (int i = 0; i < playlist.Length; i++)
            {
                var playlistmenu = new NSMenuItem();
                playlistmenu.Title = playlist[i];
                //playlistmenu.Action = new ObjCRuntime.Selector("playlist:");
                playlistmenu.Activated += (sender, e) => itunes.Playplaylist(itunes.name, playlistmenu.Title);
                menu.AddItem(playlistmenu);
            }

            //itunesの監視者を作成
            var center = (NSDistributedNotificationCenter)NSDistributedNotificationCenter.DefaultCenter;// as NSNotificationCenter;
            center.AddObserver(new NSString("com.apple.iTunes.playerInfo"), OnClockChange);

        }

        //再生曲の変更時に呼び出される関数
        public void OnClockChange(NSNotification _notification)
        {
            var dict = _notification.UserInfo;
            dict.ToList().ForEach((item) =>
            {
                Debug.WriteLine($"{item.Key} : {item.Value}");
            });
            this.item.Title = dict["Name"].ToString();
            //Console.WriteLine(dict["Name"]);
        }

    }

    public class MusicPlayer
    {
        public String name { get; }
        public String[] playlist { get; }

        public MusicPlayer(String _name)
        {
            this.name = _name;

            //プレイリストの取得
            String script = string.Format("tell application \"{0}\" to get name of user playlists", this.name);
            NSAppleEventDescriptor result = Requesttoitunes(script);

            nint B = result.NumberOfItems;
            playlist = new String[B];
            for (int i = 0; i < playlist.Length; i++)
            {
                playlist[i] = result.DescriptorAtIndex(i + 1).StringValue;
            }

        }

        //現在の曲を再生、停止する関数
        public void PlayPause()
        {
            String script = string.Format("tell application \"{0}\" to playpause", this.name);
            NSAppleEventDescriptor result = Requesttoitunes(script);
        }

        //指定されたプレイリストを再生する関数
        public void Playplaylist(String _player_name,String _music_title)
        {
            String script = "tell application \"" + _player_name + "\" to play playlist \"" + _music_title + "\"";
            NSAppleEventDescriptor result = Requesttoitunes(script);
        }

        //現在再生中のトラック名取得関数
        public String GetTrackName()
        {
            String script = string.Format("tell application \"{0}\" to get name of current track", this.name);
            NSAppleEventDescriptor result = Requesttoitunes(script);

            if (result==null)
            {
                return "Null";
            }

            return result.StringValue;
        }

        //itunesへApplescriptでリクエストを送信して結果を返す関数
        private NSAppleEventDescriptor Requesttoitunes(String _script)
        {
            NSAppleScript appleScript = new NSAppleScript(_script);
            NSDictionary error;
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);

            return result;
        }
    }

    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        NSStatusItem item;
        //MusicPlayer itunes;

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            //iTunes再生
            //NSAppleScript appleScript = new NSAppleScript("tell application \"iTunes\" to playpause");

            //再生中:kPSP 停止中:kPSp
            //NSAppleScript appleScript = new NSAppleScript("tell application \"iTunes\" to get player state");

            MusicMenu menu = new MusicMenu("Music");
            Console.OutputEncoding = Encoding.UTF8;
        }
        
        [Export("quite:")]
        public void Quite(NSObject sender)
        {
            NSApplication.SharedApplication.Terminate(this);
        }
        

        public override void WillTerminate(NSNotification notification)
        {
            
        }
    }
}
