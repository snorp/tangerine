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

namespace DAAP {

    public enum AuthenticationMethod : byte {
        None,
        UserAndPassword,
        Password,
    }
            
    internal class ServerInfo {

        private string name;
        private AuthenticationMethod authMethod;
        private bool supportsUpdate;
        
        public string Name {
            get { return name; }
            set { name = value; }
        }

        public AuthenticationMethod AuthenticationMethod {
            get { return authMethod; }
            set { authMethod = value; }
        }

        public bool SupportsUpdate {
            get { return supportsUpdate; }
            set { supportsUpdate = value; }
        }

        internal static ServerInfo FromNode (ContentNode node) {
            ServerInfo info = new ServerInfo ();

            if (node.Name != "dmap.serverinforesponse")
                return null;

            foreach (ContentNode child in (node.Value as ContentNode[])) {
                switch (child.Name) {
                case "dmap.itemname":
                    info.Name = (string) child.Value;
                    break;
                case "dmap.authenticationmethod":
                    info.AuthenticationMethod = (AuthenticationMethod) child.Value;
                    break;
                case "dmap.supportsupdate":
                    info.SupportsUpdate = (byte) child.Value == 1;
                    break;
                }
            }

            return info;
        }

        internal ContentNode ToNode (int dbCount) {
            return new ContentNode ("dmap.serverinforesponse",
                                    new ContentNode ("dmap.status", 200),
                                    new ContentNode ("dmap.protocolversion", new Version (2, 0, 2)),
                                    new ContentNode ("daap.protocolversion", new Version (3, 0, 2)),
                                    new ContentNode ("dmap.itemname", name),
                                    new ContentNode ("dmap.loginrequired", (byte) 1),
                                    new ContentNode ("dmap.authenticationmethod", (byte) authMethod),
                                    new ContentNode ("dmap.timeoutinterval", (int) Server.DefaultTimeout.TotalSeconds),
                                    new ContentNode ("dmap.supportsautologout", (byte) 1),
                                    new ContentNode ("dmap.supportsupdate", (byte) 1),
                                    new ContentNode ("dmap.supportspersistentids", (byte) 1),
                                    new ContentNode ("dmap.supportsextensions", (byte) 1),
                                    new ContentNode ("dmap.supportsbrowse", (byte) 1),
                                    new ContentNode ("dmap.supportsquery", (byte) 1),
                                    new ContentNode ("dmap.supportsindex", (byte) 1),
                                    new ContentNode ("dmap.supportsresolve", (byte) 0),
                                    new ContentNode ("dmap.databasescount", dbCount));
        }

    }
}
