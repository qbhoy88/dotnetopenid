﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.RelyingParty {
	abstract class DirectRequest {
		protected DirectRequest(ServiceEndpoint provider, IDictionary<string, string> args) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (args == null) throw new ArgumentNullException("args");
			Provider = provider;
			Args = args;
			if (Protocol.QueryDeclaredNamespaceVersion != null &&
				!Args.ContainsKey(Protocol.openid.ns))
				Args.Add(Protocol.openid.ns, Protocol.QueryDeclaredNamespaceVersion);
		}
		protected ServiceEndpoint Provider { get; private set; }
		protected Protocol Protocol { get { return Protocol.Default; } }
		protected IDictionary<string, string> Args { get; private set; }

		protected IDictionary<string, string> GetResponse() {
			byte[] body = ProtocolMessages.Http.GetBytes(Args);
			FetchResponse resp = null;
			IDictionary<string, string> args = null;
			try {
				resp = Fetcher.Request(Provider.ProviderEndpoint, body);
				args = ProtocolMessages.KeyValueForm.GetDictionary(resp.ResponseStream);
			} catch (ArgumentException e) {
				throw new OpenIdException("Failure decoding Key-Value Form response from provider.", e);
			} catch (WebException e) {
				throw new OpenIdException("Failure while connecting to provider.", e);
			}
			string mode;
			// All error codes are supposed to be returned with 400, but
			// some (like myopenid.com) sometimes send errors as 200's.
			if (resp.StatusCode == HttpStatusCode.BadRequest ||
				(args.TryGetValue(Protocol.Default.openidnp.mode, out mode) && mode == Protocol.Args.Mode.error)) {
				string providerMessage;
				args.TryGetValue(Protocol.Default.openidnp.error, out providerMessage);
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ProviderRespondedWithError, providerMessage), args);
			} else if (resp.StatusCode == HttpStatusCode.OK) {
				return args;
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ProviderRespondedWithUnrecognizedHTTPStatusCode, resp.StatusCode));
			}
		}
	}
}