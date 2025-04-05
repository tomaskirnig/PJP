using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab7
{
    public class TypeChecker
    {
        public enum DataType
        {
            Int,
            Float,
            String
        }

        private Dictionary<string, object> variables;
        private Dictionary<string, DataType> variableTypes;

        public TypeChecker()
        {
            variables = new Dictionary<string, object>();
            variableTypes = new Dictionary<string, DataType>();
        }

        public void DeclareVariable(string name, DataType type, IToken token = null)
        {
            if (variables.ContainsKey(name))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Variable '{name}' is already declared.");
            }

            variableTypes[name] = type;
            variables[name] = type == DataType.Int ? 0 : (type == DataType.Float ? 0.0f : "");
        }

        // Get variable value
        public object GetVariable(string name, IToken token = null)
        {
            if (!variables.ContainsKey(name))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Variable '{name}' is not declared.");
            }

            return variables[name];
        }

        // Get variable type
        public DataType GetVariableType(string name, IToken token = null)
        {
            if (!variableTypes.ContainsKey(name))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Variable '{name}' is not declared.");
            }

            return variableTypes[name];
        }

        // Assign value to variable with type checking
        public object AssignVariable(string name, object value, IToken token = null)
        {
            if (!variables.ContainsKey(name))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Variable '{name}' is not declared.");
            }

            DataType targetType = variableTypes[name];
            try
            {
                value = ConvertValueToType(value, targetType, token, name);
                variables[name] = value;
                return value;
            }
            catch
            {
                throw;
            }
        }

        public object ConvertValueToType(object value, DataType targetType, IToken token = null, string variableName = null)
        {
            string varInfo = !string.IsNullOrEmpty(variableName) ? $"'{variableName}' " : "";

            if (value == null)
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new ArgumentNullException(nameof(value), $"{positionInfo}Value for variable {varInfo}cannot be null.");
            }

            switch (targetType)
            {
                case DataType.Int:
                    if (value is float floatVal)
                    {
                        string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                        throw new Exception($"{positionInfo}Cannot assign float value {floatVal} to int variable {varInfo}.");
                    }
                    try
                    {
                        return Convert.ToInt32(value);
                    }
                    catch (Exception)
                    {
                        string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                        throw new Exception($"{positionInfo}Cannot convert value '{value}' to int for variable {varInfo}.");
                    }

                case DataType.Float:
                    if (value is int intValue)
                        return (float)intValue;
                    try
                    {
                        return Convert.ToSingle(value);
                    }
                    catch (Exception)
                    {
                        string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                        throw new Exception($"{positionInfo}Cannot convert value '{value}' to float for variable {varInfo}.");
                    }

                case DataType.String:
                    return value.ToString();

                default:
                    string posInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                    throw new ArgumentException($"{posInfo}Unsupported target type: {targetType} for variable {varInfo}.");
            }
        }

        // Arithmetic operations with type checking
        public object Add(object left, object right, IToken token = null)
        {
            try
            {
                if (left is string || right is string)
                {
                    return left.ToString() + right.ToString();
                }

                if (left is float || right is float)
                {
                    return Convert.ToSingle(left) + Convert.ToSingle(right);
                }

                return Convert.ToInt32(left) + Convert.ToInt32(right);
            }
            catch
            {
                throw;
            }
        }

        public object Subtract(object left, object right, IToken token = null)
        {
            try
            {
                if (left is float || right is float)
                {
                    return Convert.ToSingle(left) - Convert.ToSingle(right);
                }

                return Convert.ToInt32(left) - Convert.ToInt32(right);
            }
            catch
            {
                throw;
            }
        }

        public object Multiply(object left, object right, IToken token = null)
        {
            try
            {
                if (left is string && right is int)
                {
                    return string.Concat(Enumerable.Repeat((string)left, (int)right));
                }

                if (left is float || right is float)
                {
                    return Convert.ToSingle(left) * Convert.ToSingle(right);
                }

                return Convert.ToInt32(left) * Convert.ToInt32(right);
            }
            catch
            {
                throw;
            }
        }

        public object Divide(object left, object right, IToken token = null)
        {
            if (IsZero(right))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new DivideByZeroException($"{positionInfo}Division by zero");
            }

            try
            {
                if (left is float || right is float)
                {
                    return Convert.ToSingle(left) / Convert.ToSingle(right);
                }

                return Convert.ToInt32(left) / Convert.ToInt32(right);
            }
            catch
            {
                throw;
            }
        }

        // Modified version accepting IToken
        public object Modulo(object left, object right, IToken token = null)
        {
            if (left is float || right is float)
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Modulo operator can only be used with integers, got: '{left}' % '{right}'.");
            }

            if (IsZero(right))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new DivideByZeroException($"{positionInfo}Modulo by zero: '{left}' % 0");
            }

            try
            {
                return Convert.ToInt32(left) % Convert.ToInt32(right);
            }
            catch
            {
                throw;
            }
        }

        private bool IsZero(object value)
        {
            if (value is int intValue)
                return intValue == 0;
            if (value is float floatValue)
                return floatValue == 0;
            return false;
        }

        public DataType DetermineExpressionType(object value, IToken token = null)
        {
            if (value is int)
                return DataType.Int;
            if (value is float)
                return DataType.Float;
            if (value is string)
                return DataType.String;

            string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
            throw new ArgumentException($"{positionInfo}Unsupported value type: {value.GetType()}");
        }
    }
}
