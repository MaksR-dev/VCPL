﻿using System;
using System.Collections.Generic;
using GlobalRealization;

namespace VCPL;

public class Context
{
    public TempContainer dataContext;
    public FunctionsContainer functionsContext;

    public Context(TempContainer dataContext, FunctionsContainer functionsContext)
    {
        this.dataContext = dataContext;
        this.functionsContext = functionsContext;
    }

    public Context NewContext()
    {
        return new Context(new TempContainer(this.dataContext), new FunctionsContainer(this.functionsContext));
    }
}

public static class BasicConteext
{
    public static FunctionsContainer ElementaryFunctions = new FunctionsContainer()
    {
        {
            "print", (ref DataContainer container, int retValueId, int[] argsIds) =>
            {
                if (retValueId != -1) container[retValueId] = null;
                foreach (int arg in argsIds)
                {
                    Console.Write(container[arg]?.ToString());
                }
            }
        },
        {
            "read", (ref DataContainer container, int retValueId, int[] argsIds) =>
            {
                string value = Console.ReadLine();
                if (retValueId != -1) container[retValueId] = value;
            }
        },
        { "endl", (ref DataContainer container, int retValueId, int[] argsIds) => { Console.WriteLine(); } },
        {
            "new", (ref DataContainer container, int retValueId, int[] argsIds) =>
            {
                if (argsIds.Length == 0)
                {
                    container[retValueId] = new object() { };
                }
                else
                {
                    throw new RuntimeException("new must to get only 0 or 1 args"); // 
                }
            }
        }
    };

    public static TempContainer BasicData = new TempContainer();

    static BasicConteext()
    {
        BasicData.Push("NULL", null);
    }

    public static Context GetBasicContext()
    {
        return new Context(BasicData, ElementaryFunctions);
    }
}