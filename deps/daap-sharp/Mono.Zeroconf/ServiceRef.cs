//
// ServiceRef.cs
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
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;

namespace Mono.Zeroconf
{
    public struct ServiceRef
    {
        public static readonly ServiceRef Zero;
    
        private IntPtr raw;
    
        public ServiceRef(IntPtr raw)
        {
            this.raw = raw;
        }
    
        public void Deallocate()
        {
            Native.DNSServiceRefDeallocate(Raw);
        }
 
        public ServiceError ProcessSingle()
        {
            return Native.DNSServiceProcessResult(Raw);
        }
        
        public void Process()
        {
            while (ProcessSingle () == ServiceError.NoError) ;
        }

        public int SocketFD {
            get {
                return Native.DNSServiceRefSockFD(Raw);
            }
        }
        
        public IntPtr Raw {
            get {
                return raw;
            }
        }
        
        public override bool Equals(object o)
        {
            if(!(o is ServiceRef)) {
                return false;
            }
            
            return ((ServiceRef)o).Raw == Raw;
        }
        
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
        
        public static bool operator ==(ServiceRef a, ServiceRef b)
        {
            return a.Raw == b.Raw;
        }
        
        public static bool operator !=(ServiceRef a, ServiceRef b)
        {
            return a.Raw != b.Raw;
        }
        
        public static explicit operator IntPtr(ServiceRef value)
        {
            return value.Raw;
        }
        
        public static explicit operator ServiceRef(IntPtr value)
        {
            return new ServiceRef(value);
        }
    }
}
