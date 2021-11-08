using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDO365HealthStatus.Models
{
    public class TokenResponse
    {
        /// <summary>
        /// Gets or sets the Token_type.
        /// </summary>
        public string Token_type { get; set; }

        /// <summary>
        /// Gets or sets the Expires_in.
        /// </summary>
        public string Expires_in { get; set; }

        /// <summary>
        /// Gets or sets the Ext_expires_in.
        /// </summary>
        public string Ext_expires_in { get; set; }

        /// <summary>
        /// Gets or sets the Access_token.
        /// </summary>
        public string Access_token { get; set; }
    }
}
