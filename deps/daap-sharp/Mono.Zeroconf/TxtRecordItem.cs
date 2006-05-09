//
// TxtRecordItem.cs
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
using System.Text;

namespace Mono.Zeroconf
{
    public class TxtRecordItem
    {
        private string key;
        private byte [] value_raw;
        private string value_string;
        
        private static readonly Encoding encoding = new UTF8Encoding();
        
        public TxtRecordItem(string key, byte [] valueRaw)
        {
            this.key = key;
            ValueRaw = valueRaw;
        }
        
        public TxtRecordItem(string key, string valueString)
        {
            this.key = key;
            ValueString = valueString;
        }
        
        public override string ToString()
        {
            return String.Format("{0} = {1}", Key, ValueString);
        }
        
        public string Key {
            get { return key; }
        }
        
        public byte [] ValueRaw {
            get { return value_raw; }
            set { value_raw = value; }
        }
        
        public string ValueString {
            get { 
                if(value_string != null) {
                    return value_string;
                }
                
                value_string = encoding.GetString(value_raw);
                return value_string;
            }
            
            set {
                value_string = value;
                value_raw = encoding.GetBytes(value);
            }
        }
    }
}
