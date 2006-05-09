//
// Service.cs
//
// Authors:
//	Aaron Bockover  <abockover@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;

namespace Mono.Zeroconf
{
    public static class Zeroconf
    {
        public static void Initialize()
        {
            ServiceRef sd_ref;
            ServiceError error = Native.DNSServiceCreateConnection(out sd_ref);
            
            if(error != ServiceError.NoError) {
                throw new ServiceErrorException(error);
            }
            
            sd_ref.Deallocate();
            
            return;
        }
    }
    
    public abstract class Service
    {
        protected ServiceFlags flags = ServiceFlags.None;
        protected string name;
        protected string reply_domain;
        protected string regtype;
        protected uint interface_index;
        
        protected TxtRecord txt_record;
        protected string fullname;
        protected string hosttarget;
        protected short port;
        protected IPHostEntry hostentry;
        
        public Service()
        {
        }
        
        public Service(string name, string replyDomain, string regtype)
        {
            Name = name;
            ReplyDomain = replyDomain;
            RegType = regtype;
        }
        
        public override bool Equals(object o)
        {
            if(!(o is Service)) {
                return false;
            }
            
            return (o as Service).Name == Name;
        }
        
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        
        public ServiceFlags Flags {
            get { return flags; }
            internal set { flags = value; }
        }
        
        public uint InterfaceIndex {
            get { return interface_index; }
            set { interface_index = value; }
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }
        
        public string ReplyDomain {
            get { return reply_domain; }
            set { reply_domain = value; }
        }
        
        public string RegType {
            get { return regtype; }
            set { regtype = value; }
        }
        
        // Resolved Properties
         
        public TxtRecord TxtRecord {
            get { return txt_record; }
            set { txt_record = value; }
        }
               
        public string FullName { 
            get { return fullname; }
            internal set { fullname = value; }
        }
        
        public string HostTarget {
            get { return hosttarget; }
        }
        
        public IPHostEntry HostEntry {
            get { return hostentry; }
        }
        
        public short Port {
            get { return IPAddress.NetworkToHostOrder(port); }
            set { port = IPAddress.HostToNetworkOrder(value); }
        }
    }
}
