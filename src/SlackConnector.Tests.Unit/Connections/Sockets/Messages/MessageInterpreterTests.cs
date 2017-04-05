﻿using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using Should;
using SlackConnector.Connections.Models;
using SlackConnector.Connections.Sockets.Messages.Inbound;
using SlackConnector.Logging;
using SpecsFor.ShouldExtensions;

namespace SlackConnector.Tests.Unit.Connections.Sockets.Messages
{
    internal class MessageInterpreterTests
    {
        [Test, AutoMoqData]
        public void should_return_standard_message(MessageInterpreter interpreter)
        {
            // given
            string json = @"
                {
                  'type': 'message',
                  'channel': '&lt;myChannel&gt;',
                  'user': '&lt;myUser&gt;',
                  'text': 'hi, my name is &lt;noobot&gt;',
                  'ts': '1445366603.000002',
                  'team': '&lt;myTeam&gt;'
                }
            ";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            var expected = new ChatMessage
            {
                MessageType = MessageType.Message,
                Channel = "<myChannel>",
                User = "<myUser>",
                Text = "hi, my name is <noobot>",
                Team = "<myTeam>",
                RawData = json,
                Timestamp = 1445366603.000002
            };

            result.ShouldLookLike(expected);
        }

        [Test, AutoMoqData]
        public void should_return_group_joined_message(MessageInterpreter interpreter)
        {
            // given
            string json = @"
                {
                  'type': 'group_joined',
                  'channel': {
                    id: 'test-group',
                    is_group: true
                  }
                }
            ";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            var expected = new GroupJoinedMessage
            {
                MessageType = MessageType.Group_Joined,
                Channel = new Group
                {
                    Id = "test-group",
                    IsGroup = true
                },
                RawData = json,
            };

            result.ShouldLookLike(expected);
        }

        [Test, AutoMoqData]
        public void should_return_channel_joined_message(MessageInterpreter interpreter)
        {
            // given
            string json = @"
                {
                  'type': 'channel_joined',
                  'channel': {
                    id: 'test-channel',
                    is_channel: true
                  }
                }
            ";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            var expected = new ChannelJoinedMessage
            {
                MessageType = MessageType.Channel_Joined,
                Channel = new Channel
                {
                    Id = "test-channel",
                    IsChannel = true
                },
                RawData = json,
            };

            result.ShouldLookLike(expected);
        }

        [Test, AutoMoqData]
        public void should_return_dm_channel_joined_message(MessageInterpreter interpreter)
        {
            // given
            string json = @"
                {
                   'type':'im_created',
                   'channel':{
                      'id':'D99999',
                      'user':'U99999',
                      'is_im':true,
                      'is_open':true
                   }
                }
            ";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            var expected = new DmChannelJoinedMessage
            {
                MessageType = MessageType.Im_Created,
                Channel = new Im
                {
                    Id = "D99999",
                    User = "U99999",
                    IsIm = true,
                    IsOpen = true
                },
                RawData = json,
            };

            result.ShouldLookLike(expected);
        }

        [Test, AutoMoqData]
        public void should_return_user_joined_message(MessageInterpreter interpreter)
        {
            // given
            string json = @"
                {  
                   'type':'team_join',
                   'user':{  
                      'id':'U3339999',
                      'name':'tmp',
                      'profile':{  
                         'real_name':'temp-name'
                      },
                      'is_admin':false,
                      'is_bot':true
                   }
                }
            ";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            var expected = new UserJoinedMessage
            {
                MessageType = MessageType.Team_Join,
                User = new User
                {
                    Id = "U3339999",
                    Name = "tmp",
                    IsAdmin = false,
                    IsBot = true,
                    Profile = new Profile
                    {
                        RealName = "temp-name"
                    }
                },
                RawData = json,
            };

            result.ShouldLookLike(expected);
        }

        [Test, AutoMoqData]
        public void should_return_unknown_message_type(MessageInterpreter interpreter)
        {
            // given
            string json = @"{ 'type': 'something_else' }";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            var expected = new ChatMessage
            {
                MessageType = MessageType.Unknown,
                RawData = json
            };

            result.ShouldLookLike(expected);
        }

        [Test, AutoMoqData]
        public void shouldnt_return_message_given_dodge_json(MessageInterpreter interpreter)
        {
            // given
            string json = @"{ 'type': 'something_else', 'channel': { 'isObject': true } }";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            result.ShouldBeNull();
        }

        [Test, AutoMoqData]
        public void should_return_message_given_standard_message_with_null_data(MessageInterpreter interpreter)
        {
            // given
            string json = @"
                {
                  'type': 'message',
                  'channel': null,
                  'user': null,
                  'text': null,
                  'ts': '1445366603.000002',
                  'team': null
                }
            ";

            // when
            var result = interpreter.InterpretMessage(json);

            // then
            var expected = new ChatMessage
            {
                MessageType = MessageType.Message,
                Channel = null,
                User = null,
                Text = null,
                Team = null,
                RawData = json,
                Timestamp = 1445366603.000002
            };

            result.ShouldLookLike(expected);
        }

        [Test, AutoMoqData]
        public void shouldnt_log_when_logging_level_is_non([Frozen]Mock<ILogger> logger, MessageInterpreter interpreter)
        {
            // given
            SlackConnector.LoggingLevel = ConsoleLoggingLevel.None;

            // when
            var result = interpreter.InterpretMessage(null);

            // then
            result.ShouldBeNull();
            logger.Verify(x => x.LogError(It.IsAny<string>()), Times.Never);
        }

        [Test, AutoMoqData]
        public void should_log_when_logging_level_is_all([Frozen]Mock<ILogger> logger, MessageInterpreter interpreter)
        {
            // given
            SlackConnector.LoggingLevel = ConsoleLoggingLevel.All;

            // when
            var result = interpreter.InterpretMessage(null);

            // then
            result.ShouldBeNull();
            logger.Verify(x => x.LogError(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}