// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace NuGet.Services.Configuration.Tests
{
    public class ConfigurationRootSecretReaderFactoryFacts
    {
        [Fact]
        public void ConstructorThrowsWhenKeyConfigIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationRootSecretReaderFactory(null));
        }

        public static IEnumerable<object[]> InvalidConfigs
        {
            get
            {
                yield return new object[] {
                    new Dictionary<string, string> {
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultVaultNameKey, "KeyVaultName" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultUseManagedIdentity, "true" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultClientIdKey, "KeyVaultClientId" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultCertificateThumbprintKey, "KeyVaultThumbprint" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreNameKey, "StoreName"},
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreLocationKey, "StoreLocation" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultValidateCertificateKey, "true" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultSendX5c, "false" }
                    }
                };
            }
        }

        public static IEnumerable<object[]> ValidConfigs
        {
            get
            {
                yield return new object[] {
                    new Dictionary<string, string> {
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultVaultNameKey, "KeyVaultName" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultUseManagedIdentity, "true" },
                    }
                };

                yield return new object[] {
                    new Dictionary<string, string> {
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultClientIdKey, "KeyVaultClientId" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultCertificateThumbprintKey, "KeyVaultThumbprint" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreNameKey, "StoreName"},
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreLocationKey, "StoreLocation" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultValidateCertificateKey, "true" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultSendX5c, "false" }
                    }
                };

                yield return new object[] {
                    new Dictionary<string, string> {
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultVaultNameKey, "KeyVaultName" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultUseManagedIdentity, "false" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultClientIdKey, "KeyVaultClientId" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultCertificateThumbprintKey, "KeyVaultThumbprint" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreNameKey, "StoreName"},
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreLocationKey, "StoreLocation" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultValidateCertificateKey, "true" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultSendX5c, "false" }
                    }
                };

                yield return new object[] {
                    new Dictionary<string, string> {
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultVaultNameKey, "KeyVaultName" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultUseManagedIdentity, "false" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultClientIdKey, "" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultCertificateThumbprintKey, "" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreNameKey, ""},
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultStoreLocationKey, "" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultValidateCertificateKey, "" },
                        { Constants.DefaultKeyVaultPrefix + Constants.KeyVaultSendX5c, "" }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidConfigs))]
        public void ConstructorThrowsWhenKeyVaultConfigSpecifiesManagedIdentityAndCertificate(IDictionary<string, string> config)
        {
            Assert.Throws<ArgumentException>(() => new ConfigurationRootSecretReaderFactory(CreateTestConfiguration(config)));
        }

        [Theory]
        [MemberData(nameof(ValidConfigs))]
        public void CreatesSecretReaderFactoryForValidConfiguration(IDictionary<string, string> config)
        {
            var secretReaderFactory = new ConfigurationRootSecretReaderFactory(CreateTestConfiguration(config));
            Assert.NotNull(secretReaderFactory);
        }


        private IConfigurationRoot CreateTestConfiguration(IDictionary<string, string> config)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();
        }
    }
}
