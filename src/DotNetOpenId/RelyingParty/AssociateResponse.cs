﻿using System;
using System.Collections.Generic;
using System.Text;
using Org.Mentalis.Security.Cryptography;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	class AssociateResponse : DirectResponse {
		public AssociateResponse(ServiceEndpoint provider, IDictionary<string, string> args, DiffieHellman dh)
			: base(provider, args) {
			DH = dh;

			if (Args.ContainsKey(Protocol.openidnp.assoc_handle)) {
				initializeAssociation();
			} else {
				// Attempt to recover from an unsupported assoc_type
				if (Protocol.Version.Major >= 2) {
					if (Util.GetRequiredArg(Args, Protocol.openidnp.error_code) == Protocol.Args.ErrorCode.UnsupportedType) {
						string assoc_type = Util.GetRequiredArg(Args, Protocol.openidnp.assoc_type);
						string session_type = Util.GetRequiredArg(Args, Protocol.openidnp.session_type);
						// If the suggested options are among those we support...
						if (Array.IndexOf(Protocol.Args.SignatureAlgorithm.All, assoc_type) >= 0 &&
							Array.IndexOf(Protocol.Args.SessionType.All, session_type) >= 0) {
							SecondAttempt = AssociateRequest.Create(Provider, assoc_type, session_type);
						}
					}
				}
			}
		}

		void initializeAssociation() {
			string assoc_type = Util.GetRequiredArg(Args, Protocol.openidnp.assoc_type);
			if (Protocol.Args.SignatureAlgorithm.HMAC_SHA1.Equals(assoc_type, StringComparison.Ordinal) ||
				Protocol.Args.SignatureAlgorithm.HMAC_SHA256.Equals(assoc_type, StringComparison.Ordinal)) {
				byte[] secret;

				string session_type;
				if (!Args.TryGetValue(Protocol.openidnp.session_type, out session_type) ||
					Protocol.Args.SessionType.NoEncryption.Equals(session_type, StringComparison.Ordinal)) {
					secret = getDecoded(Protocol.openidnp.mac_key);
				} else if (Protocol.Args.SessionType.DH_SHA1.Equals(session_type, StringComparison.Ordinal)) {
					byte[] dh_server_public = getDecoded(Protocol.openidnp.dh_server_public);
					byte[] enc_mac_key = getDecoded(Protocol.openidnp.enc_mac_key);
					secret = CryptUtil.SHAHashXorSecret(CryptUtil.Sha1, DH, dh_server_public, enc_mac_key);
				} else if (Protocol.Args.SessionType.DH_SHA256.Equals(session_type, StringComparison.Ordinal)) {
					byte[] dh_server_public = getDecoded(Protocol.openidnp.dh_server_public);
					byte[] enc_mac_key = getDecoded(Protocol.openidnp.enc_mac_key);
					secret = CryptUtil.SHAHashXorSecret(CryptUtil.Sha256, DH, dh_server_public, enc_mac_key);
				} else {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValue,
						Protocol.openid.session_type, session_type));
				}

				string assocHandle = Util.GetRequiredArg(Args, Protocol.openidnp.assoc_handle);
				TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(Util.GetRequiredArg(Args, Protocol.openidnp.expires_in), CultureInfo.CurrentUICulture));

				if (assoc_type == Protocol.Args.SignatureAlgorithm.HMAC_SHA1) {
					Association = new HmacSha1Association(assocHandle, secret, expiresIn);
				} else if (assoc_type == Protocol.Args.SignatureAlgorithm.HMAC_SHA256) {
					Association = new HmacSha256Association(assocHandle, secret, expiresIn);
				} else {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValue,
						Protocol.openid.assoc_type, assoc_type));
				}
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue,
					Protocol.openid.assoc_type, assoc_type));
			}
		}
		public DiffieHellman DH { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		public Association Association { get; private set; }

		byte[] getDecoded(string key) {
			try {
				return Convert.FromBase64String(Util.GetRequiredArg(Args, key));
			} catch (FormatException ex) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ExpectedBase64OpenIdQueryParameter, key), null, ex);
			}
		}

		/// <summary>
		/// A custom-made associate request to try again when an OP
		/// doesn't support the settings we suggested, but we support
		/// the ones the OP suggested.
		/// </summary>
		public AssociateRequest SecondAttempt { get; private set; }
	}
}