using Antlr4.Runtime;
using System;
using System.Collections.Generic;

namespace PLC_Lab7
{
    public class TypeChecker
    {
        public enum DataType
        {
            Int,
            Float,
            String,
            Bool
        }

        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Dictionary<string, DataType> variableTypes = new Dictionary<string, DataType>();

        public List<string> TypeErrors { get; } = new List<string>();

        // Declare a new variable with a given type. If already declared, add an error.
        public void DeclareVariable(string name, DataType type, IToken token = null)
        {
            if (variableTypes.ContainsKey(name))
            {
                string pos = token != null ? $"{token.Line}:{token.Column} - " : "";
                TypeErrors.Add($"{pos}Variable '{name}' is already declared.");
                return;
            }

            variableTypes[name] = type;
            // Initialize with the default value for the declared type.
            switch (type)
            {
                case DataType.Int:
                    variables[name] = 0;
                    break;
                case DataType.Float:
                    variables[name] = 0.0f;
                    break;
                case DataType.Bool:
                    variables[name] = false;
                    break;
                case DataType.String:
                    variables[name] = "";
                    break;
            }
        }

        // Retrieve the variable's current value.
        public object GetVariable(string name, IToken token = null)
        {
            if (!variables.ContainsKey(name))
            {
                string pos = token != null ? $"{token.Line}:{token.Column} - " : "";
                TypeErrors.Add($"{pos}Variable '{name}' is not declared.");
                return null;
            }
            return variables[name];
        }

        // Return the declared type of a variable.
        public DataType GetVariableType(string name, IToken token = null)
        {
            if (!variableTypes.ContainsKey(name))
            {
                string pos = token != null ? $"{token.Line}:{token.Column} - " : "";
                TypeErrors.Add($"{pos}Variable '{name}' is not declared.");
                return DataType.Int; // fallback
            }
            return variableTypes[name];
        }

        // Attempt to assign a value to a variable.
        // If the conversion fails or the types are mismatched, an error is added.
        public object AssignVariable(string name, object value, IToken token = null)
        {
            if (!variableTypes.ContainsKey(name))
            {
                string pos = token != null ? $"{token.Line}:{token.Column} - " : "";
                TypeErrors.Add($"{pos}Variable '{name}' is not declared.");
                return null;
            }

            DataType target = variableTypes[name];
            try
            {
                value = ConvertValueToType(value, target, token, name);
                variables[name] = value;
                Console.WriteLine($"Assigned value: {value} to variable: {name}");
            }
            catch (Exception ex)
            {
                TypeErrors.Add(ex.Message);
            }
            return value;
        }

        // Convert the given value to the target type.
        public object ConvertValueToType(object value, DataType target, IToken token = null, string varName = null)
        {
            string pos = token != null ? $"{token.Line}:{token.Column} - " : "";
            if (value == null)
                throw new ArgumentNullException("value", $"{pos}Value for variable '{varName}' cannot be null.");

            // Determine the source type
            DataType sourceType;
            if (value is int) sourceType = DataType.Int;
            else if (value is float) sourceType = DataType.Float;
            else if (value is bool) sourceType = DataType.Bool;
            else if (value is string) sourceType = DataType.String;
            else throw new Exception($"{pos}Unsupported value type: {value.GetType()} for variable '{varName}'.");

            if (sourceType == target)
                return value;

            switch (target)
            {
                case DataType.Int:
                    throw new Exception($"{pos}Cannot convert {sourceType} to int for variable '{varName}'.");

                case DataType.Float:
                    if (sourceType == DataType.Int)
                        return (float)((int)value);
                    throw new Exception($"{pos}Cannot convert {sourceType} to float for variable '{varName}'.");

                case DataType.Bool:
                    if (sourceType == DataType.Int)
                        return ((int)value) != 0;
                    if (sourceType == DataType.Float)
                        return ((float)value) != 0.0f;
                    throw new Exception($"{pos}Cannot convert {sourceType} to bool for variable '{varName}'.");

                case DataType.String:
                    throw new Exception($"{pos}Cannot convert {sourceType} to string for variable '{varName}'.");

                default:
                    throw new Exception($"{pos}Unsupported target type for variable '{varName}'.");
            }
        }

        // Determine the type of an expression based on the runtime value.
        // Throws an error if the value is null or of an unsupported type.
        public DataType DetermineExpressionType(object value, IToken token = null)
        {
            string pos = token != null ? $"{token.Line}:{token.Column} - " : "";
            if (value == null)
            {
                // Místo vyhození výjimky vrátit výchozí typ a přidat chybu do seznamu
                TypeErrors.Add($"{pos}Expression value is null. Using Int as fallback type.");
                return DataType.Int; // Fallback typ
            }

            if (value is int)
                return DataType.Int;
            if (value is float)
                return DataType.Float;
            if (value is bool)
                return DataType.Bool;
            if (value is string)
                return DataType.String;

            throw new Exception($"{pos}Unsupported value type: {value.GetType()}");
        }
    }
}
