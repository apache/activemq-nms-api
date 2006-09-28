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
using NUnit.Framework;
using System;
using System.Messaging;

namespace MSMQ
{    
    /// <summary>
    /// Use to test and verify MSMQ behaviour.
    /// </summary>
	[TestFixture]
    public class MSMQTest
    {
        String queueName = ".\\Private$\\FOO";
	    
		[SetUp]
        public void SetUp()
        {
        }
		
        [TearDown]
        public void TearDown()
        {
        }		
		
        [Test]
        public void TestSendAndReceive()
        {
            // check to make sure the message queue does not exist already
            if (!MessageQueue.Exists(queueName))
            {
                // create the new message queue and make it transactional
                MessageQueue MQ = MessageQueue.Create(queueName, true);

                // set the label name and close the message queue
                MQ.Label = "FOO";
                MQ.Close();

                Console.WriteLine("Created Queue: " + queueName);
                //Assert.Fail("Should  have thrown an exception!");
            } 
            else
            {
                Console.WriteLine("Queue Existed: " + queueName);
                
            }


            if (!MessageQueue.Exists(".\\Private$\\BAR"))
            {
                // create the new message queue and make it transactional
                MessageQueue MQ = MessageQueue.Create(".\\Private$\\BAR", true);

                // set the label name and close the message queue
                MQ.Label = "BAR Label";
                MQ.Close();

            }
            else
            {
                Console.WriteLine("Queue Existed: " + queueName);

            }

            // create a message queue transaction and start it
            MessageQueueTransaction Transaction = new MessageQueueTransaction();
            Transaction.Begin();

            MessageQueue MQueue = new MessageQueue(queueName);

            Message Msg = new Message("Hello World");
            Msg.ResponseQueue = new MessageQueue(".\\Private$\\BAR");
            Msg.Priority = MessagePriority.Normal;
            Msg.UseJournalQueue = true;
            Msg.Label = "Test Label";

            Msg.AcknowledgeType = AcknowledgeTypes.FullReceive;
            Msg.AdministrationQueue = Msg.ResponseQueue;

            // send the message
            MQueue.Send(Msg, Transaction);
            MQueue.Send(Msg, Transaction);
            MQueue.Send(Msg, Transaction);

            // commit the transaction
            Transaction.Commit();

            
            // Read the message.
            MQueue.MessageReadPropertyFilter.SetAll();
            
            // the target type we have stored in the message body
            
            ((XmlMessageFormatter)MQueue.Formatter).TargetTypes = new Type[] { typeof(String) };
            
              // read the message from the queue, but only wait for 5 sec
              Msg = MQueue.Receive(new TimeSpan(0, 0, 5));

              // read the order from the message body
              Console.WriteLine("Received: "+Msg.Body);
            

            // close the mesage queue
            MQueue.Close();
		}
						
    }
}



