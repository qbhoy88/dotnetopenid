﻿// <auto-generated />

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

	[ContractClassFor(typeof(AssociateSuccessfulResponse))]
	internal abstract class AssociateSuccessfulResponseContract : AssociateSuccessfulResponse {
		protected AssociateSuccessfulResponseContract() : base(null, null) {
		}

		protected override Association CreateAssociationAtProvider(AssociateRequest request, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
			throw new NotImplementedException();
		}

		protected override Association CreateAssociationAtRelyingParty(AssociateRequest request) {
			Contract.Requires<ArgumentNullException>(request != null);
			throw new NotImplementedException();
		}
	}
}
