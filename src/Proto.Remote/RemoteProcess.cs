﻿// -----------------------------------------------------------------------
//  <copyright file="RemoteProcess.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using System;
using Google.Protobuf;

namespace Proto.Remote
{
    public class RemoteProcess : Process
    {
        private readonly PID _pid;

        public RemoteProcess(PID pid)
        {
            _pid = pid;
        }

        public override void SendUserMessage(PID pid, object message, PID sender)
        {
            Send(pid, message, sender);
        }

        public override void SendSystemMessage(PID pid, object message)
        {
            Send(pid, message, null);
        }

        private void Send(PID pid, object msg, PID sender)
        {
            if (msg is Watch w)
            {
                var rw = new RemoteWatch(w.Watcher, _pid);
                RemotingSystem.EndpointManagerPid.Tell(rw);
            }
            else if (msg is Unwatch uw)
            {
                var ruw = new RemoteUnwatch(uw.Watcher, _pid);
                RemotingSystem.EndpointManagerPid.Tell(ruw);
            }
            else
            {
                SendRemoteMessage(_pid, msg, sender);
            }
        }

        public static void SendRemoteMessage(PID pid, object msg, PID sender)
        {
            if (msg is IMessage)
            {
                var imsg = (IMessage) msg;
                var env = new MessageEnvelope
                {
                    Target = pid,
                    Sender = sender,
                    MessageData = Serialization.Serialize(imsg),
                    TypeName = imsg.Descriptor.File.Package + "." + imsg.Descriptor.Name
                };
                RemotingSystem.EndpointManagerPid.Tell(env);
            }
            else
            {
                throw new NotSupportedException("Non protobuf message");
            }
        }
    }
}