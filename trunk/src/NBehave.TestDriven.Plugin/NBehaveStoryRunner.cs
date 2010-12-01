﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using NBehave.Narrator.Framework;
using NBehave.Narrator.Framework.Text;
using TestDriven.Framework;

//IF you change the namespace or the name of the class dont forget to update the installer.
namespace NBehave.TestDriven.Plugin
{
    public class NBehaveStoryRunner : ITestRunner
    {
        private IRunner _runner;

        TestRunState ITestRunner.RunAssembly(ITestListener tddNetListener, Assembly assembly)
        {
            return Run(assembly, null, tddNetListener);            
        }

        TestRunState ITestRunner.RunMember(ITestListener tddNetListener, Assembly assembly, MemberInfo member)
        {
            return Run(assembly, member, tddNetListener);            
        }

        TestRunState ITestRunner.RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            //TODO: Fix filter. ns is probably equal to NamespaceFilter in StoryRunnerFilter
            return Run(assembly, null, testListener);      
        }

        private TestRunState Run(Assembly assembly, MemberInfo member, ITestListener tddNetListener)
        {
            var listener = new StoryRunnerEventListenerProxy(tddNetListener);
            _runner = RunnerFactory.CreateTextRunner(null, listener);
            
            var locator = new StoryLocator
                            {
                                RootLocation = Path.GetDirectoryName(assembly.Location)
                            };

            var type = member as Type;
            IEnumerable<string> stories = (type == null)
                                      ? locator.LocateAllStories() 
                                      : locator.LocateStoriesMatching(type);
            
            _runner.Load(stories);
            _runner.StoryRunnerFilter = StoryRunnerFilter.GetFilter(member);
            _runner.LoadAssembly(assembly.CodeBase);

            FeatureResults results = _runner.Run(new PlainTextOutput(Console.Out));

            return GetTestRunState(results);
        }

        private static TestRunState GetTestRunState(FeatureResults results)
        {
            TestRunState state = TestRunState.Success;
            if (results.NumberOfFailingScenarios > 0)
                state = TestRunState.Failure;
            if (results.NumberOfScenariosFound == 0)
                state = TestRunState.NoTests;
            return state;
        }
    }
}
