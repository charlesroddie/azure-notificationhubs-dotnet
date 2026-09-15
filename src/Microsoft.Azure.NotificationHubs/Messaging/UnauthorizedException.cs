//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//-----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.NotificationHubs.Messaging
{
    /// <summary> Exception for signaling authorization errors. </summary>
    public class UnauthorizedException : MessagingException
    {
        /// <summary> Constructor. </summary>
        /// <param name="detail"> Detail about the cause of the exception. </param>
        internal UnauthorizedException(MessagingExceptionDetail detail) :
            base(detail, false)
        {            
        }
    }
}
