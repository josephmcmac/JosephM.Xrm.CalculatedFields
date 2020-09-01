﻿using System.Activities;

namespace JosephM.Xrm.CalculatedFields.Plugins.Xrm
{
    public abstract class XrmWorkflowActivityRegistration : CodeActivity
    {
        protected override sealed void Execute(CodeActivityContext executionContext)
        {
            var instance = CreateInstance();
            instance.ExecuteBase(executionContext, this);
        }

        protected abstract XrmWorkflowActivityInstanceBase CreateInstance();
    }
}