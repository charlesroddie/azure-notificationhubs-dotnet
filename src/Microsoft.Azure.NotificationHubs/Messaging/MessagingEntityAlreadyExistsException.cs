//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//-----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.NotificationHubs.Messaging
{
    /// <summary> Exception for signalling messaging entity already exists errors. </summary>
    public sealed class MessagingEntityAlreadyExistsException : MessagingException
    {
        /// <summary> Constructor. </summary>
        /// <param name="detail"> Detail about the cause of the exception. </param>
        internal MessagingEntityAlreadyExistsException(MessagingExceptionDetail detail) :
            base(detail, false)
        {
        }

        /// <summary> Constructor. </summary>
        /// <param name="detail"> Detail about the cause of the exception. </param>
        /// <param name="innerException"> The inner exception. </param>
        internal MessagingEntityAlreadyExistsException(MessagingExceptionDetail detail, Exception innerException) :
            base(detail, false, innerException)
        {
        }
    }
}
