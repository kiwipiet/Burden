using System;
using System.Diagnostics.CodeAnalysis;
using FakeItEasy;
using Xunit;

namespace EPS.Concurrency.Tests.Unit
{
	[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes", Justification = "Non-issue for test classes really")]
	public abstract class JobResultInspectorTestBase<TJobInput, TJobOutput, TQueuePoison>
	{
		private Func<IJobResultInspector<TJobInput, TJobOutput, TQueuePoison>> _factory;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics, while advanced, are perfectly acceptable within Funcs")]
		protected JobResultInspectorTestBase(Func<IJobResultInspector<TJobInput, TJobOutput, TQueuePoison>> factory)
		{
			this._factory = factory;
		}

		[Fact]
		public void Inspect_ThrowsOnNullNotification()
		{
			var instance = _factory();
			Assert.Throws<ArgumentNullException>(() => instance.Inspect(null));
		}

		[Fact]
		public void Inspect_ReturnsNonNullResult_OnNonNullInput()
		{
			var result = _factory().Inspect(JobResult.CreateOnCompletion(A.Dummy<TJobInput>(), A.Dummy<TJobOutput>()));

			Assert.NotNull(result);
		}

		private JobQueueAction<TQueuePoison> GetErrorInspectionResults(Exception exception)
		{
			var jobResult = JobResult.CreateOnError(A.Dummy<TJobInput>(), exception);
			return _factory().Inspect(jobResult);
		}

		[Fact]
		public void Inspect_ReturnsPoisonQueueAction_ForErrorJobResult_WithNonNullJobResult_NonNullException()
		{
			var result = GetErrorInspectionResults(new ArgumentNullException());
			Assert.Equal(JobQueueActionType.Poison, result.ActionType);
		}

		[Fact]
		public void Inspect_ReturnsPoisonQueueAction_ForErrorJobResult_WithNonNullJobResult_NullException()
		{
			var result = GetErrorInspectionResults(null);
			Assert.Equal(JobQueueActionType.Poison, result.ActionType);
		}
	}
}