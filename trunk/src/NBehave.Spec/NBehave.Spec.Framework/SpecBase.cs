﻿using System;
using Rhino.Mocks;

namespace NBehave.Spec
{
	public abstract class SpecBase
	{
		private MockRepository _mocks;

		public virtual void MainSetup()
		{
			_mocks = new MockRepository();

			Before_each_spec();
		}

		public virtual void MainTeardown()
		{
			After_each_spec();
		}

		protected virtual void Before_each_spec() {}

		protected virtual void After_each_spec() {}

		protected MockRepository Mocks
		{
			get { return _mocks; }
		}

		protected IDisposable RecordExpectedBehavior
		{
			get { return _mocks.Record(); }
		}

		protected IDisposable PlaybackBehavior
		{
			get { return _mocks.Playback(); }
		}

		protected TType CreateDependency<TType>()
		{
			return MockRepository.GenerateMock<TType>();
		}

		protected TType CreateStub<TType>()
		{
			return MockRepository.GenerateStub<TType>();
		}

		protected TType Partial<TType>()
		   where TType : class
		{
			return _mocks.PartialMock<TType>();
		}

		protected void VerifyAll()
		{
			_mocks.VerifyAll();
		}

		protected void Spec_not_implemented()
		{
			Console.WriteLine("Not implemented");
		}
	}
}
