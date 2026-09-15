//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//-----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.NotificationHubs.Messaging
{
    /// <summary> Exception for signaling quota exceeded errors. </summary>
    public class QuotaExceededException : MessagingException
    {
        internal readonly TimeSpan DefaultRetryTimeout = TimeSpan.FromSeconds(10);

        /// <summary> Constructor. </summary>
        /// <param name="detail"> Detail about the cause of the exception. </param>
        /// <param name="retryAfter">Retry after value.</param>
        internal QuotaExceededException(MessagingExceptionDetail detail, TimeSpan? retryAfter) :
            base(detail, true)
        {
            RetryAfter = retryAfter ?? DefaultRetryTimeout;
        }
    }
}
