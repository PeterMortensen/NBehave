﻿using NUnit.Framework;

namespace NBehave.Spec.NUnit
{
	[TestFixture]
	public abstract class SpecBase : Spec.SpecBase
	{
		[SetUp]
		public override void MainSetup()
		{
			base.MainSetup();
		}

		[TearDown]
		public override void MainTeardown()
		{
			base.MainTeardown();
		}
	}
}