﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NBehave.Narrator.Framework
{
    public class ActionMethodInfo : ActionMatch
    {
        public string ActionType { get; set; } //Given, When Then etc..
        public MethodInfo MethodInfo { get; set; }
    }

    public class ActionStepRunner : RunnerBase
    {
        private readonly List<string> _scenarios = new List<string>();
        private readonly ActionStepAlias _actionStepAlias = new ActionStepAlias();
        private readonly ActionStep _actionStep;
        private readonly ActionStepFileLoader _actionStepFileLoader;

        public ActionCatalog ActionCatalog { get; private set; }

        public ActionStepRunner()
        {
            ActionCatalog = new ActionCatalog();
            StoryRunnerFilter = new StoryRunnerFilter();
            _actionStepFileLoader = new ActionStepFileLoader(_actionStepAlias);
            _actionStep = new ActionStep(_actionStepAlias);
        }

        protected override void ParseAssembly(Assembly assembly)
        {
            ActionStepParser parser = new ActionStepParser(StoryRunnerFilter, ActionCatalog, _actionStepAlias);
            parser.FindActionSteps(assembly);
        }

        protected override void RunStories(StoryResults results, IEventListener listener)
        {
            listener.ThemeStarted(string.Empty);
            RunScenarios(results, listener);
            listener.StoryResults(results);
            listener.ThemeFinished();
            ClearStoryList();
        }

        private void RunScenarios(StoryResults storyResults, IEventListener listener)
        {
            var story = new Story(string.Empty) { IsDryRun = IsDryRun };
            int scenarioCounter = 0;
            foreach (string scenarioText in _scenarios)
            {
                scenarioCounter++;
                RunScenario(story, scenarioText, storyResults, listener, scenarioCounter);
            }
        }

        private void RunScenario(Story story, string scenarioText, StoryResults storyResults, IEventListener listener,
                                 int scenarioCounter)
        {
            var textToTokenStringsParser = new TextToTokenStringsParser(_actionStepAlias);

            textToTokenStringsParser.ParseScenario(scenarioText);
            string scenarioTitle = string.Format("Scenario {0}", scenarioCounter);
            var scenarioResult = new ScenarioResults(string.Empty, scenarioTitle);
            foreach (var row in textToTokenStringsParser.TokenStrings)
            {
                if (_actionStep.IsStoryTitle(row))
                {
                    story.Title = _actionStep.GetTitle(row);
                    scenarioResult.StoryTitle = story.Title;
                }
                else if (_actionStep.IsNarrative(row))
                    story.Narrative += row;
                else if (Scenario.IsScenarioTitle(row))
                    scenarioResult.ScenarioTitle = Scenario.GetTitle(row);
                else
                    RunScenario(row, scenarioResult);

            }
            var scenario = new Scenario(scenarioResult.ScenarioTitle, story);
            story.AddScenario(scenario);
            listener.ScenarioMessageAdded(textToTokenStringsParser.ScenarioMessage());
            storyResults.AddResult(scenarioResult);
        }

        private void RunScenario(string row, ScenarioResults scenarioResult)
        {
            try
            {
                string rowWithoutActionType = row.RemoveFirstWord();
                if (ActionCatalog.ActionExists(rowWithoutActionType) == false)
                    scenarioResult.Pend(string.Format("No matching Action found for \"{0}\"", row));
                else
                    InvokeTokenString(rowWithoutActionType);
            }
            catch (Exception e)
            {
                Exception realException = FindUsefulException(e);
                scenarioResult.Fail(realException);
            }
        }

        private Exception FindUsefulException(Exception e)
        {
            Exception realException = e;
            while (realException != null && realException.GetType() == typeof(TargetInvocationException))
            {
                realException = realException.InnerException;
            }
            if (realException == null)
                return e;
            return realException;
        }

        public void InvokeTokenString(string tokenString)
        {
            if (ActionCatalog.ActionExists(tokenString) == false)
                throw new ArgumentException(string.Format("cannot find Token string '{0}'", tokenString));

            object action = ActionCatalog.GetAction(tokenString).Action;

            Type actionType = action.GetType().IsGenericType
                ? action.GetType().GetGenericTypeDefinition()
                : action.GetType();
            MethodInfo methodInfo = actionType.GetMethod("DynamicInvoke");
            object[] actionParamValues = ActionCatalog.GetParametersForMessage(tokenString);

            methodInfo.Invoke(action, BindingFlags.InvokeMethod, null,
                              new object[] { actionParamValues }, CultureInfo.CurrentCulture);
        }

        public void Load(IEnumerable<string> scenarioLocations)
        {
            var scenarios = _actionStepFileLoader.Load(scenarioLocations);
            _scenarios.AddRange(scenarios);
        }

        public void Load(Stream stream)
        {
            var scenarios = _actionStepFileLoader.Load(stream);
            _scenarios.AddRange(scenarios);
        }

        private void InvokeActionBehaviour(string actionStep)
        {

        }
    }
}