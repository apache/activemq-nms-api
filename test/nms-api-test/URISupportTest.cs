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

using System;
using System.Collections.Specialized;
using Apache.NMS.Util;
using NUnit.Framework;
#if !NETCF
using System.Web;

#endif

namespace Apache.NMS.Test
{
    [TestFixture()]
    public class URISupportTest
    {
        protected void AssertMapKey(StringDictionary map, String key, Object expected)
        {
            Assert.AreEqual(expected, map[key], "Map key: " + key);
        }

        [Test]
        public void TestCreateSupportedUriVariations(
            [Values("tcp://127.0.0.1:61616",
                "tcp:127.0.0.1:61616",
                "failover:tcp://127.0.0.1:61616",
                "failover:(tcp://127.0.0.1:61616)",
                "failover://(tcp://127.0.0.1:61616)",
                "failover://(tcp://127.0.0.1:61616,tcp:192.168.0.1:61616)",
                "activemq:failover:(tcp://localhost:61616)",
                "activemq:failover:(tcp://${activemqhost}:61616)")]
            string uriString)
        {
            Uri result = URISupport.CreateCompatibleUri(NMSTestSupport.ReplaceEnvVar(uriString));

            Assert.IsNotNull(result);
        }

        [Test]
        public void TestParseCompositeWithFragment()
        {
            string uriString =
                "failover:(tcp://localhost:61616,ssl://remotehost:61617?param=true#fragment)#fragment";

            URISupport.CompositeData rc = URISupport.ParseComposite(new Uri(uriString));

            Assert.IsTrue(rc.Components.Length == 2);
            Assert.AreEqual("failover", rc.Scheme);
            Assert.AreEqual("#fragment", rc.Fragment);

            Uri uri1 = rc.Components[0];
            Uri uri2 = rc.Components[1];

            Assert.AreEqual("tcp", uri1.Scheme);
            Assert.AreEqual("ssl", uri2.Scheme);
            Assert.AreEqual("localhost", uri1.Host);
            Assert.AreEqual("remotehost", uri2.Host);
            Assert.AreEqual(61616, uri1.Port);
            Assert.AreEqual(61617, uri2.Port);
            Assert.IsTrue(String.IsNullOrEmpty(uri1.Fragment));
            Assert.IsNotNull(uri2.Fragment);
            Assert.AreEqual("#fragment", uri2.Fragment);
            Assert.AreEqual("?param=true", uri2.Query);
        }

        [Test]
        public void TestCreateRemainingUriNonComposite()
        {
            string uriStringNoParams = "tcp://localhost:61616";
            string uriStringWithParams1 = "tcp://localhost:61616?param1=true&param2=false";
            string uriStringWithParams2 = "tcp://localhost:61616?param2=false&param1=true";

            Uri uriNoParams = new Uri(uriStringNoParams);
            Uri uriWithParams1 = new Uri(uriStringWithParams1);

            Uri result = URISupport.CreateRemainingUri(uriNoParams, null);
            Assert.AreEqual(uriStringNoParams, result.OriginalString);

            result = URISupport.CreateRemainingUri(uriWithParams1, null);
            Assert.AreEqual(uriStringNoParams, result.OriginalString);

            StringDictionary parameters = new StringDictionary();
            parameters.Add("param1", "true");
            parameters.Add("param2", "false");

            result = URISupport.CreateRemainingUri(uriNoParams, parameters);
            // Have to test for reordering of parameters.  The StringDictionary that is used internally
            // does not guarantee ordering of parameters.  Out of order parameters are equivalent.
            // We just want to test that they were created successfully.
            bool isEqual = (0 == string.Compare(uriStringWithParams1, result.OriginalString)
                            || 0 == string.Compare(uriStringWithParams2, result.OriginalString));
            Assert.IsTrue(isEqual, string.Format("Error creating remaining Uri: {0}", result.OriginalString));
        }

        [Test]
        public void TestCreateRemainingUriComposite()
        {
            string uriStringNoParams = "failover:tcp://localhost:61616";
            string uriStringWithParams1 = "failover:tcp://localhost:61616?param1=true&param2=false";
            string uriStringWithParams2 = "failover:tcp://localhost:61616?param2=false&param1=true";

            Uri uriNoParams = new Uri(uriStringNoParams);
            Uri uriWithParams = new Uri(uriStringWithParams1);

            Uri result = URISupport.CreateRemainingUri(uriNoParams, null);
            Assert.AreEqual(uriStringNoParams, result.OriginalString);

            result = URISupport.CreateRemainingUri(uriWithParams, null);
            Assert.AreEqual(uriStringNoParams, result.OriginalString);

            StringDictionary parameters = new StringDictionary();
            parameters.Add("param1", "true");
            parameters.Add("param2", "false");

            result = URISupport.CreateRemainingUri(uriNoParams, parameters);
            bool isEqual = (0 == string.Compare(uriStringWithParams1, result.OriginalString)
                            || 0 == string.Compare(uriStringWithParams2, result.OriginalString));
            Assert.IsTrue(isEqual, string.Format("Error creating remaining Uri: {0}", result.OriginalString));

            // Now test a Composite with parens.

            uriStringNoParams = "failover:(tcp://localhost:61616)";
            uriStringWithParams1 = "failover:(tcp://localhost:61616)?param1=true&param2=false";
            uriStringWithParams2 = "failover:(tcp://localhost:61616)?param2=false&param1=true";

            uriNoParams = new Uri(uriStringNoParams);
            uriWithParams = new Uri(uriStringWithParams1);

            result = URISupport.CreateRemainingUri(uriNoParams, null);
            Assert.AreEqual(uriStringNoParams, result.OriginalString);

            result = URISupport.CreateRemainingUri(uriWithParams, null);
            Assert.AreEqual(uriStringNoParams, result.OriginalString);

            result = URISupport.CreateRemainingUri(uriNoParams, parameters);
            isEqual = (0 == string.Compare(uriStringWithParams1, result.OriginalString)
                       || 0 == string.Compare(uriStringWithParams2, result.OriginalString));
            Assert.IsTrue(isEqual, string.Format("Error creating remaining Uri: {0}", result.OriginalString));

            string queryString = URISupport.CreateQueryString(parameters);
            Assert.AreEqual(uriNoParams.OriginalString + "?" + queryString, result.OriginalString);
        }

        [Test]
        public void TestEmptyCompositePath()
        {
            URISupport.CompositeData data = URISupport.ParseComposite(new Uri("broker:()/localhost?persistent=false"));
            Assert.AreEqual(0, data.Components.Length);
        }

        [Test]
        public void TestCompositePath()
        {
            URISupport.CompositeData data = URISupport.ParseComposite(new Uri("test:(test:path)/path"));
            Assert.AreEqual("path", data.Path);
            data = URISupport.ParseComposite(new Uri("test:test:path"));
            Assert.IsNull(data.Path);
        }

        [Test]
        public void TestSimpleComposite()
        {
            URISupport.CompositeData data = URISupport.ParseComposite(new Uri("test:tcp://part1"));
            Assert.AreEqual(1, data.Components.Length);
        }

        [Test]
        public void TestComposite()
        {
            URISupport.CompositeData data = URISupport.ParseComposite(
                new Uri("test:(part1://host,part2://(sub1://part,sub2:part))"));
            Assert.AreEqual(2, data.Components.Length);
        }

        [Test]
        public void TestCompositeWithComponentParam()
        {
            URISupport.CompositeData data =
                URISupport.ParseComposite(new Uri("test:(part1://host?part1=true)?outside=true"));
            Assert.AreEqual(1, data.Components.Length);
            Assert.AreEqual(1, data.Parameters.Count);
            StringDictionary part1Params = URISupport.ParseParameters(data.Components[0]);
            Assert.AreEqual(1, part1Params.Count);
            Assert.IsTrue(part1Params.ContainsKey("part1"));
        }

        [Test]
        public void TestParsingURI()
        {
            Uri source = new Uri("tcp://localhost:61626/foo/bar?cheese=Edam&x=123");
            StringDictionary map = URISupport.ParseParameters(source);

            Assert.AreEqual(2, map.Count, "Size: " + map);
            AssertMapKey(map, "cheese", "Edam");
            AssertMapKey(map, "x", "123");

            Uri result = URISupport.RemoveQuery(source);
            Assert.AreEqual(new Uri("tcp://localhost:61626/foo/bar"), result);
        }

        [Test]
        public void TestParsingCompositeURI()
        {
            URISupport.CompositeData data = URISupport.ParseComposite(
                URISupport.CreateCompatibleUri("failover://(tcp://localhost:61616)?name=foo"));

            Assert.AreEqual(1, data.Components.Length, "one component");
            Assert.AreEqual("localhost", data.Components[0].Host, "Component Host is incorrect");
            Assert.AreEqual(61616, data.Components[0].Port, "Component Port is incorrect");
            Assert.AreEqual(1, data.Parameters.Count, "Size: " + data.Parameters);
        }

        [Test]
        public void TestCheckParenthesis()
        {
            String str = "fred:(((ddd))";
            Assert.IsFalse(URISupport.CheckParenthesis(str));
            str += ")";
            Assert.IsTrue(URISupport.CheckParenthesis(str));
        }

        [Test]
        public void TestParseQueury()
        {
            String query1 = "?param1=false&param2=true";
            String query2 = "param3=false&param4=true&param5=foo";

            StringDictionary results = URISupport.ParseQuery(query1);

            Assert.IsTrue(results.Count == 2);
            Assert.AreEqual("false", results["param1"]);
            Assert.AreEqual("true", results["param2"]);

            results = URISupport.ParseQuery(query2);

            Assert.IsTrue(results.Count == 3);
            Assert.AreEqual("false", results["param3"]);
            Assert.AreEqual("true", results["param4"]);
            Assert.AreEqual("foo", results["param5"]);

            String query3 = "?param";

            try
            {
                URISupport.ParseQuery(query3);
                Assert.Fail("Should have thrown an Exception on invalid parameter.");
            }
            catch
            {
            }
        }

        [Test]
        public void TestCreateWithQuery()
        {
            Uri source = new Uri("vm://localhost");
            Uri dest = URISupport.CreateUriWithQuery(source, "network=true&one=two");

            Assert.AreEqual(2, URISupport.ParseParameters(dest).Count, "correct param count");
            Assert.AreEqual(source.Host, dest.Host, "same uri, host");
            Assert.AreEqual(source.Scheme, dest.Scheme, "same uri, scheme");
            Assert.IsFalse(dest.Query.Equals(source.Query), "same uri, ssp");
        }

#if !NETCF
        [Test]
        public void TestParseQueryEncoding()
        {
            String paramName = "name";
            String paramValue = "CN=Test, OU=bla, ..&%/§()%3q743847)/(&%/.. hjUIFHUFH";

            String uriString = "http://someserver.com:1234/?";

            //encoding the param with url encode
            uriString += paramName + "=" + HttpUtility.UrlEncode(paramValue);

            Uri uri = new Uri(uriString);

            StringDictionary dictionary = URISupport.ParseQuery(uri.Query);

            String value = dictionary[paramName];

            NUnit.Framework.Assert.AreEqual(paramValue, value);
        }
#endif
    }
}