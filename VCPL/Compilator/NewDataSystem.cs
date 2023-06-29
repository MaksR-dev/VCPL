﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using GlobalRealization;

namespace VCPL;

public enum Contexts
{
    Constant = 0,
    Variable = 1
}

public class new_Context
{
    public TempContainer DataContext;
    public TempConstantContainer Constants;

    
    public new_Context(TempContainer dataContext, TempConstantContainer constants)
    {
        this.DataContext = dataContext;
        this.Constants = constants;
    }

    public Pointer PushConstant(object data)
    {
        int position = Constants.Push(data); 
        return new Pointer((byte)Contexts.Constant, position);
    }

    public Pointer PushData(string name, object data)
    {
        int position = DataContext.Push(name, data);
        return new Pointer((byte)Contexts.Variable, position);
    }

    public Pointer Peek(string name)
    {
        return new Pointer((byte)Contexts.Variable, DataContext.Peek(name));
    }
}

public struct Pointer
{
    private byte ContextType;
    private int Position;

    public byte GetContextType
    {
        get { return this.ContextType; }
    }
    
    public int GetPosition
    {
        get { return this.Position; }
    }
    
    public Pointer(byte contextType, int position)
    {
        this.ContextType = contextType;
        this.Position = position;
    }
}

public class TempConstantContainer
{
    private List<object> data;
    private TempConstantContainer Context;
    
    private int counter;
    
    public int Size
    {
        get { return (this.Context?.Size ?? 0) + this.data.Count; }
    }
    public TempConstantContainer()
    {
        this.data = new List<object>();
        counter = 0;
        this.Context = null;
    }
    public TempConstantContainer(TempConstantContainer context)
    {
        this.data = new List<object>();
        counter = context.Size;
        this.Context = context;
    }
    
    public int Push(object value)
    {
        this.data.Add(value);
        return counter++;
    }

    public ConstantContainer Pack()
    {
        return new ConstantContainer(data);
    }
}

public class ConstantContainer
{
    private object[] _data;
    private ConstantContainer Context;

    public int Size
    {
        get { return (this.Context?.Size ?? 0) + this._data.Length; }
    }
    
    public ConstantContainer(List<object> objects)
    {
        this._data = new object[objects.Count];
        for(int i = 0; i < objects.Count; i++)
        {
            this._data[i] = objects[i];
        }
        Context = null;
    }

    public void SetContext(ConstantContainer context)
    {
        this.Context = context;
    }

    public object this[int index]
    {
        get { return (index < (this.Context?.Size ?? 0) ? this.Context[index] : this._data[index - (this.Context?.Size ?? 0)]); }
    }
}

public class DataContainer
{
    private object[] _data;
    private DataContainer Context;

    public DataContainer(int size)
    {
        this._data = new object[size];
        Context = null;
    }

    public void SetContext(DataContainer context)
    {
        this.Context = context;
    }

    public object this[int index]
    {
        get { return (index < (Context?.Size ?? 0) ? this.Context[index] : this._data[index - (Context?.Size ?? 0)]); }
        set
        {
            if (index < (Context?.Size ?? 0))
            {
                this.Context[index] = value;
            }
            else
            {
                this._data[index - (Context?.Size ?? 0)] = value;
            }
        }
    }

    public int Size
    {
        get { return (this.Context?.Size ?? 0) + this._data.Length; }
    }

    public DataContainer GetCopy()
    {
        DataContainer copy = new DataContainer(_data.Length);
        
        for (int i = 0; i < _data.Length; i++)
        {
            copy[i] = Copy(this._data[i]);
        }
        copy.SetContext(this.Context);

        return copy;
    }

    public static object Copy(object item)
    {
        if (item == null) return null;
        if (item is byte byteItem) return byteItem;
        if (item is char charItem) return charItem;
        if (item is bool boolItem) return boolItem;
        if (item is int intItem) return intItem;
        if (item is double doubleItem) return doubleItem;
        if (item is string stringItem) return new string(stringItem);
        return item; // it should be error // imposible to init not based types
    }
}

public class TempContainer
{
    private List<(string name, object value)> data;
    private TempContainer Context;
    
    private int counter;
    
    public int Size
    {
        get { return (this.Context?.Size ?? 0) + this.data.Count; }
    }
    public TempContainer()
    {
        this.data = new List<(string name, object value)>();
        counter = 0;
        this.Context = null;
    }
    public TempContainer(TempContainer context)
    {
        this.data = new List<(string name, object value)>();
        counter = context.Size;
        this.Context = context;
    }
    
    public int Push(string name, object value)
    {
        if (name == null)
        {
            throw new CompilationException("Variable name was null");
        }
        
        for (int i = 0; i < this.data.Count; i++) 
            if (this.data[i].name == name) 
                throw new ArgumentException();

        this.data.Add((name, value));
        return counter++;
    }

    public int Peek(string name)
    {
        for (int i = 0; i < this.data.Count; i++)
            if (this.data[i].name == name)
                return i + this.Context?.Size ?? 0;

        return this.Context?.Peek(name) ?? -1;
    }
    
    public DataContainer Pack()
    {
        DataContainer container = new DataContainer(data.Count);
        
        for (int i = 0; i < this.data.Count; i++) 
            container[i] = this.data[i].value;
        
        if (this.Context != null) 
            container.SetContext(this.Context.Pack()); /////////////////////////

        return container;
    }
}