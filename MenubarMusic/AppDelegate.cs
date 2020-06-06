using AppKit;
using Foundation;
using System;
using System.Text;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using CoreGraphics;

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

            /*
            NSSlider slider = new NSSlider(new CGRect(10, 0, 100, 20));
            slider.MaxValue = 50.0F;
            slider.MinValue = 0.0F;
            slider.FloatValue = 25.0F;
            slider.SetFrameSize(new CGSize(160, 16));
            sliderItem.View = slider;
            menu.AddItem(sliderItem);
            */

            //Pre、Nextボタン作成
            var tableItem = new NSMenuItem();
            tableItem.Title = "table";
            NSGridView table = new NSGridView(new CGRect(0, 0, 90, 16));
            NSButton [] text = {new NSButton(new CGRect(0, 0, 50, 10)), new NSButton(new CGRect(0, 0, 20, 10))};
            //NSTextField [] text = { new NSTextField(new CGRect(0, 0, 40, 10)), new NSTextField(new CGRect(0, 0, 20, 10)) };

            text[0].Title = "Pre";
            text[0].Alignment = NSTextAlignment.Center;
            //text[0].Bordered = false;
            text[0].Activated += (sender, e) => itunes.PreTrack();

            text[1].Title = "Next";
            text[1].Alignment = NSTextAlignment.Center;
            //text[1].Bordered = false;
            text[1].Activated += (sender, e) => itunes.NextTrack();

            table.AddRow(text);
            tableItem.View = table;
            menu.AddItem(tableItem);

            //プレイリストをメニューに追加
            var playlist = new NSMenuItem();
            playlist.Title = "iTunes PlayList";
            menu.AddItem(playlist);
            NSMenu playlistsubmenu = new NSMenu();
            playlist.Submenu = playlistsubmenu;


            var playlist_name = itunes.playlist;

            for (int i = 0; i < playlist_name.Length; i++)
            {
                var playlistsubmenuitem = new NSMenuItem();
                playlistsubmenuitem.Title = playlist_name[i];
                //playlistmenu.Action = new ObjCRuntime.Selector("playlist:");
                playlistsubmenuitem.Activated += (sender, e) => itunes.Playplaylist(itunes.name, playlistsubmenuitem.Title);
                playlistsubmenu.AddItem(playlistsubmenuitem);
                
            }

            menu.AddItem(NSMenuItem.SeparatorItem);

            //アプリ終了項目追加
            var menuItem = new NSMenuItem();
            menuItem.Title = "Quit";
            menuItem.Action = new ObjCRuntime.Selector("quite:");
            menu.AddItem(menuItem);

            var sliderItem = new NSMenuItem();
            sliderItem.Title = "slider";

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

        //次の曲に移動
        public void PreTrack()
        {
            String script = string.Format("tell application \"{0}\" to previous track", this.name);
            NSAppleEventDescriptor result = Requesttoitunes(script);
        }

        //前の曲に移動
        public void NextTrack()
        {
            String script = string.Format("tell application \"{0}\" to next track", this.name);
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
        //NSStatusItem item;
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
