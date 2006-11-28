using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Nini;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    [Plugin ("lsongs")]
    public class LsongsPlugin : IDisposable {

        private Dictionary<string, Track> tracks = new Dictionary<string, Track> ();
        private string dbpath;
        private string lsongsDir;
        private CreationWatcher watcher;
        
        public LsongsPlugin () {
            dbpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), "Documents/Music/Lsongs/LsongsMusicLibrary.xml");
            lsongsDir = Path.GetDirectoryName (dbpath);

            if (Directory.Exists (lsongsDir)) {
                Init ();
            } else {
                watcher = new CreationWatcher (lsongsDir);
                watcher.Created += delegate {
                    watcher.Dispose ();
                    watcher = null;
                    Init ();
                };
            }
        }

        private void Init () {
            RefreshTracks ();

            if (Inotify.Enabled) {
                Inotify.Subscribe (lsongsDir, OnLsongsChanged, Inotify.EventType.MovedTo);
            }
        }

        private void ClearTracks () {
            foreach (Track track in tracks.Values) {
                Daemon.DefaultDatabase.RemoveTrack (track);
            }

            tracks.Clear ();
        }

        private void RefreshTracks () {
            ClearTracks ();
            
            if (!File.Exists (dbpath)) {
                return;
            }
            
            XmlTextReader reader = new XmlTextReader (dbpath);
	    reader.WhitespaceHandling = WhitespaceHandling.None;

	    int found_tracks = 0;
	    int found_id = 0;
            int count = 0;

            Track track = new Track ();

	    while (reader.Read()){
	      if (found_tracks == 0){
	        if (reader.LocalName == "key"){
		  if (reader.ReadString() == "Tracks"){
		    found_tracks = 1;
		    continue;
		  }
		  else{
		    continue;
		  }
		}
		else{
		  continue;
		}
	      }
             
              if (reader.LocalName == "key") {
	        if (found_id == 0){
		  found_id = 1;
		  continue;
		}
		else{
		  string value = reader.ReadString();
		  switch (value){
		  case "Album":
		    reader.Read();
		    track.Album = reader.ReadString();
		    break;
		  case "Track Number":
		    reader.Read();
		    track.TrackNumber = Int32.Parse(reader.ReadString());
		    break;
		  case "Location":
		    reader.Read();
                    track.FileName = reader.ReadString();
		    break;
		  case "Name":
		    reader.Read();
		    track.Title = reader.ReadString();
		    break;
		  case "Artist":
		    reader.Read();
		    track.Artist = reader.ReadString();
		    break;
		  case "Total Time":
		    reader.Read();
		    track.Duration = TimeSpan.FromMilliseconds(Int32.Parse(reader.ReadString()));
		    break;
		  default:
		    break;
		  }
		}
	      }
	      else if(reader.LocalName == "dict" && reader.NodeType == XmlNodeType.EndElement){
	        found_id = 0; 
		if (track.FileName != null){
		  Daemon.DefaultDatabase.AddTrack (track);
		  tracks[track.FileName] = track;
		}
                track = new Track ();
		count++;
		continue;
	      }
	      else{
	        continue;
	      }
	    }
            reader.Close ();
        }

        public void Dispose () {
            if (watcher != null) {
                watcher.Dispose ();
                watcher = null;
            }
        }

        private void OnLsongsChanged (Inotify.Watch watch, string path, string subitem,
                                         string srcpath, Inotify.EventType type) {
            string file = Path.Combine (path, subitem);

            if (file == dbpath) {
                RefreshTracks ();
                Daemon.Server.Commit ();
            } 
        }
    }
}
