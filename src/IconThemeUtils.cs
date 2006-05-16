
/***************************************************************************
 *  IconThemeUtils.cs
 *
 *  Copyright (C) 2005 Novell
 *  Written by Aaron Bockover (aaron@aaronbock.net)
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using System.Runtime.InteropServices;
using Gdk;
using GLib;

namespace Tangerine
{
    public static class IconThemeUtils
    {
        [DllImport("libgtk-win32-2.0-0.dll")]
        private extern static IntPtr gtk_icon_theme_get_default();

        [DllImport("libgtk-win32-2.0-0.dll")]
        private extern static IntPtr gtk_icon_theme_load_icon(IntPtr theme, string name, int size, 
            int flags, IntPtr error);
            
        [DllImport("libgtk-win32-2.0-0.dll")]
        private static extern bool gtk_icon_theme_has_icon(IntPtr theme, string name);

        public static bool HasIcon(string name)
        {
            return gtk_icon_theme_has_icon(gtk_icon_theme_get_default(), name);
        }

        public static Gdk.Pixbuf LoadIcon(int size, params string [] names)
        {
            for(int i = 0; i < names.Length; i++) {
                Gdk.Pixbuf pixbuf = LoadIcon(names[i], size, i == names.Length - 1);
                if(pixbuf != null) {
                    return pixbuf;
                }
            }
            
            return null;
        }

        public static Gdk.Pixbuf LoadIcon(string name, int size)
        {
            return LoadIcon(name, size, true);
        }

        public static Gdk.Pixbuf LoadIcon(string name, int size, bool fallBackOnResource)
        {
            try {
                IntPtr native = gtk_icon_theme_load_icon(gtk_icon_theme_get_default(), name, size, 0, IntPtr.Zero);
                if(native != IntPtr.Zero) {
                    Gdk.Pixbuf ret = (Gdk.Pixbuf)GLib.Object.GetObject(native, true);
                    if(ret != null) {
                        return ret;
                    }
                }
            } catch(Exception) {
            }
            
            if(!fallBackOnResource) {
                return null;
            }
            
            try {
                return new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), name + ".png");
            } catch(Exception) {
                return null;
            }
        }

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_class_peek(IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_class_find_property(IntPtr klass, string name);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set(IntPtr obj, string property, IntPtr value, IntPtr nullarg);

        public static void SetWindowIcon(Gtk.Window window)
        {
            SetWindowIcon(window, "tangerine");
        }

        public static void SetWindowIcon(Gtk.Window window, string iconName)
        {
            GType gtype = (GType)typeof(Gtk.Window);
            IntPtr klass = g_type_class_peek(gtype.Val);
            IntPtr property = g_object_class_find_property(klass, "icon-name");

            if(property != IntPtr.Zero) {
                IntPtr str_ptr = GLib.Marshaller.StringToPtrGStrdup(iconName);
                g_object_set(window.Handle, "icon-name", str_ptr, IntPtr.Zero);
                GLib.Marshaller.Free(str_ptr);
            }
        }
    }
}
