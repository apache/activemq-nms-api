/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#define TRACE // Force tracing to be enabled for this class

namespace Apache.NMS.Test
{
    public class NmsTracer : Apache.NMS.ITrace
    {
        #region ITrace Members

        public void Debug(string message)
        {
#if !NETCF
            System.Diagnostics.Trace.WriteLine(string.Format("DEBUG: {0}", message));
#endif
        }

        public void Error(string message)
        {
#if !NETCF
            System.Diagnostics.Trace.WriteLine(string.Format("ERROR: {0}", message));
#endif
        }

        public void Fatal(string message)
        {
#if !NETCF
            System.Diagnostics.Trace.WriteLine(string.Format("FATAL: {0}", message));
#endif
        }

        public void Info(string message)
        {
#if !NETCF
            System.Diagnostics.Trace.WriteLine(string.Format("INFO: {0}", message));
#endif
        }

        public void Warn(string message)
        {
#if !NETCF
            System.Diagnostics.Trace.WriteLine(string.Format("WARN: {0}", message));
#endif
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        #endregion
    }
}