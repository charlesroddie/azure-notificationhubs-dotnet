//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//-----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.NotificationHubs.Messaging
{
    /// <summary> Exception for signaling bad request data errors. </summary>
    public class BadRequestException : MessagingException
    {
        /// <summary> Constructor. </summary>
        /// <param name="detail"> Detail about the cause of the exception. </param>
        internal BadRequestException(MessagingExceptionDetail detail) :
            base(detail, false)
        {            
        }
    }
}
