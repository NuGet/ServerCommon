﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NuGet.Services.Validation.Issues.Tests {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NuGet.Services.Validation.Issues.Tests.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {&quot;c&quot;:&quot;NU3008&quot;,&quot;m&quot;:&quot;The package integrity check failed.&quot;}.
        /// </summary>
        internal static string ClientSigningVerificationFailureIssueJson {
            get {
                return ResourceManager.GetString("ClientSigningVerificationFailureIssueJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {}.
        /// </summary>
        internal static string EmptyJson {
            get {
                return ResourceManager.GetString("EmptyJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {&quot;A&quot;:&quot;Hello&quot;,&quot;B&quot;:123}.
        /// </summary>
        internal static string ObsoleteTestingIssueJson {
            get {
                return ResourceManager.GetString("ObsoleteTestingIssueJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {&quot;t&quot;:&quot;thumbprint-sha256&quot;,&quot;o&quot;:&quot;1.2.3.4.5.6&quot;}.
        /// </summary>
        internal static string UnauthorizedAzureTrustedSigningCertificateFailureIssueJson {
            get {
                return ResourceManager.GetString("UnauthorizedAzureTrustedSigningCertificateFailureIssueJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {&quot;t&quot;:&quot;thumbprint&quot;}.
        /// </summary>
        internal static string UnauthorizedCertificateFailureIssueJson {
            get {
                return ResourceManager.GetString("UnauthorizedCertificateFailureIssueJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {&quot;t&quot;:&quot;thumbprint-sha256&quot;}.
        /// </summary>
        internal static string UnauthorizedCertificateSha256FailureIssueJson {
            get {
                return ResourceManager.GetString("UnauthorizedCertificateSha256FailureIssueJson", resourceCulture);
            }
        }
    }
}
