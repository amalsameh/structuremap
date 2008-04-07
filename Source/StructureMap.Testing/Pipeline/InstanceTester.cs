using System;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap.Interceptors;
using StructureMap.Pipeline;
using StructureMap.Testing.Container.Interceptors;

namespace StructureMap.Testing.Pipeline
{
    [TestFixture]
    public class InstanceTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Instance_Build_Calls_into_its_Interceptor()
        {
            MockRepository mocks = new MockRepository();
            InstanceInterceptor interceptor = mocks.CreateMock<InstanceInterceptor>();
            InstanceInterceptor interceptor2 = mocks.CreateMock<InstanceInterceptor>();
            StructureMap.Pipeline.IInstanceCreator instanceCreator = mocks.CreateMock<StructureMap.Pipeline.IInstanceCreator>();


            InstanceUnderTest instanceUnderTest = new InstanceUnderTest();
            instanceUnderTest.Interceptor = interceptor;

            object objectReturnedByInterceptor = new object();
            using (mocks.Record())
            {
                Expect.Call(interceptor.Process(instanceUnderTest.TheInstanceThatWasBuilt)).Return(objectReturnedByInterceptor);
                Expect.Call(instanceCreator.FindInterceptor(instanceUnderTest.TheInstanceThatWasBuilt.GetType())).Return
                    (interceptor2);

                Expect.Call(interceptor2.Process(objectReturnedByInterceptor)).Return(objectReturnedByInterceptor);
            }

            using (mocks.Playback())
            {
                Assert.AreEqual(objectReturnedByInterceptor, instanceUnderTest.Build(typeof(object), instanceCreator));
                
            }
        }

        #endregion
    }

    public class InstanceUnderTest : Instance
    {
        public object TheInstanceThatWasBuilt = new object();


        protected override object build(Type type, StructureMap.Pipeline.IInstanceCreator creator)
        {
            return TheInstanceThatWasBuilt;
        }
    }
}