using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Metatool.Service;

public interface IContextVariable
{
	void NewGeneration();
	void AddVariable(string key, Func<Task<object>> generator);
	Task<T> GetVariable<T>(string key, bool force=false);
}