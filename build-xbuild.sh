#!/usr/bin/env bash

# Licensed to the Apache Software Foundation (ASF) under one or more
# contributor license agreements.  See the NOTICE file distributed with
# this work for additional information regarding copyright ownership.
# The ASF licenses this file to You under the Apache License, Version 2.0
# (the "License"); you may not use this file except in compliance with
# the License.  You may obtain a copy of the License at
# 
# http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.


# 
# A simple shell script for building on Mono using XBuild.
#
# WARNING: At the time of writing XBuild on Mono does not handle cross-project references
# so it can only manage to build the NMS.dll :(
#

echo "Building NMS.dll"
xbuild vs2005-nms.csproj

# echo "Building NMS-test.dll"
# xbuild vs2005-nms-test.csproj

# echo "Building ActiveMQ.dll"
# xbuild vs2005-activemq.csproj

#echo "Building ActiveMQ-test.dll"
#xbuild vs2005-activemq.csproj
