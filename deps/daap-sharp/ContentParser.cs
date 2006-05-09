/*
 * daap-sharp
 * Copyright (C) 2005  James Willcox <snorp@snorp.net>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Text;
using System.Net;
using System.Collections;

namespace DAAP {

    public class ContentException : ApplicationException {

        public ContentException (string msg) : base (msg) {
        }
    }

    internal class ContentNode {
        public string Name;
        public object Value;

        public ContentNode () {
        }
        
        public ContentNode (string name, params object[] values) {
            this.Name = name;

            ArrayList vals = new ArrayList ();
            foreach (object v in values) {
                if (v is ICollection) {
                    vals.AddRange ((ICollection) v);
                } else {
                    vals.Add (v);
                }
            }

            if (vals.Count == 1 && vals[0].GetType () != typeof (ContentNode))
                this.Value = vals[0];
            else
                this.Value = (object) vals.ToArray (typeof (ContentNode));
        }

        public void Dump () {
            Dump (0);
        }
        
        private void Dump (int level) {
            Console.WriteLine ("{0}Name: {1}", String.Empty.PadRight (level * 4), Name);

            if (Value is ContentNode[]) {
                foreach (ContentNode child in (Value as ContentNode[])) {
                    child.Dump (level + 1);
                }
            } else {
                Console.WriteLine ("{0}Value ({1}): {2}", String.Empty.PadRight (level * 4),
                                   Value.GetType (), Value);
                Console.WriteLine ();
            }
        }

        public ContentNode GetChild (string name) {
            if (name == this.Name)
                return this;

            ContentNode[] children = Value as ContentNode[];
            if (children == null)
                return null;

            foreach (ContentNode child in children) {
                ContentNode needle = child.GetChild (name);
                if (needle != null)
                    return needle;
            }

            return null;
        }
    }

    internal class ContentParser {

        private static ContentNode[] ParseChildren (ContentCodeBag bag, byte[] buffer,
                                                    int offset, int length) {
            ArrayList children = new ArrayList ();

            int position = offset;

            while (position < offset + length) {
                children.Add (Parse (bag, buffer, null, ref position));
            }

            return (ContentNode[]) children.ToArray (typeof (ContentNode));
        }

        public static ContentNode Parse (ContentCodeBag bag, byte[] buffer, string root,
                                         ref int offset) {
            ContentNode node = new ContentNode ();

            int num = IPAddress.NetworkToHostOrder (BitConverter.ToInt32 (buffer, offset));
            ContentCode code = bag.Lookup (num);
            if (code.Equals (ContentCode.Zero)) {
                // probably a buggy server.  fallback to our internal code bag
                code = ContentCodeBag.Default.Lookup (num);

                if (code.Equals (ContentCode.Zero)) {
                    throw new ContentException ("Failed to find content code for: " + num);
                }
            }

            int length = IPAddress.NetworkToHostOrder (BitConverter.ToInt32 (buffer, offset + 4));

            node.Name = code.Name;

            switch (code.Type) {
            case ContentType.Char:
                node.Value = (byte) buffer[offset + 8];
                break;
            case ContentType.Short:
                node.Value = IPAddress.NetworkToHostOrder (BitConverter.ToInt16 (buffer, offset + 8));
                break;
            case ContentType.SignedLong:
            case ContentType.Long:
                node.Value = IPAddress.NetworkToHostOrder (BitConverter.ToInt32 (buffer, offset + 8));
                break;
            case ContentType.LongLong:
                node.Value = IPAddress.NetworkToHostOrder (BitConverter.ToInt64 (buffer, offset + 8));
                break;
            case ContentType.String:
                node.Value = Encoding.UTF8.GetString (buffer, offset + 8, length);
                break;
            case ContentType.Date:
                node.Value = Utility.ToDateTime (IPAddress.NetworkToHostOrder (BitConverter.ToInt32 (buffer, offset + 8)));
                break;
            case ContentType.Version:
                int major = IPAddress.NetworkToHostOrder (BitConverter.ToInt16 (buffer, offset + 8));
                int minor = (int) buffer[offset + 10];
                int micro = (int) buffer[offset + 11];

                node.Value = new Version (major, minor, micro);
                break;
            case ContentType.Container:
                node.Value = ParseChildren (bag, buffer, offset + 8, length);
                break;
            default:
                throw new ContentException (String.Format ("Unknown content type '{0}' for '{1}'",
                                                           code.Type, code.Name));
            }

            offset += length + 8;

            if (root != null) {
                ContentNode rootNode = node.GetChild (root);

                if (rootNode == null)
                    throw new ContentException (String.Format ("Could not find root node '{0}'", root));

                return rootNode;
            } else {
                return node;
            }
        }
        
        public static ContentNode Parse (ContentCodeBag bag, byte[] buffer, string root) {
            int offset = 0;
            return Parse (bag, buffer, root, ref offset);
        }

        public static ContentNode Parse (ContentCodeBag bag, byte[] buffer) {
            return Parse (bag, buffer, null);
        }
    }
}
