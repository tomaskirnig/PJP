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
            String,
            Bool
        }

        private Dictionary<string, object> variables;
        private Dictionary<string, DataType> variableTypes;

        public TypeChecker()
        {
            variables = new Dictionary<string, object>();
            variableTypes = new Dictionary<string, DataType>();
        }

        // Declare variable
        public void DeclareVariable(string name, DataType type, IToken token = null)
        {
            if (variables.ContainsKey(name))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Variable '{name}' is already declared.");
            }

            variableTypes[name] = type;

            // Initialize the variable with a default value based on its type
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

                case DataType.Bool:
                    if (value is bool)
                        return value;
                    if (value is int intVal)
                        return intVal != 0;
                    try
                    {
                        return Convert.ToBoolean(value);
                    }
                    catch (Exception)
                    {
                        string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                        throw new Exception($"{positionInfo}Cannot convert value '{value}' to boolean for variable {varInfo}.");
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
            if (value is bool)
                return DataType.Bool;

            string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
            throw new ArgumentException($"{positionInfo}Unsupported value type: {value.GetType()}");
        }
        public object Equal(object left, object right, IToken token = null)
        {
            try
            {
                if (left is string && right is string)
                {
                    return (string)left == (string)right;
                }
                else if ((left is int || left is float) && (right is int || right is float))
                {
                    return Convert.ToDouble(left) == Convert.ToDouble(right);
                }
                else if (left is bool || right is bool)
                {
                    if (left is bool && right is bool)
                        return (bool)left == (bool)right;
                    else if (left is bool && (right is int || right is float))
                    {
                        // Numeric to boolean (0 = false, anything else = true)
                        double numValue = Convert.ToDouble(right);
                        return (bool)left == (numValue != 0);
                    }
                    else if ((left is int || left is float) && right is bool)
                    {
                        double numValue = Convert.ToDouble(left);
                        return (numValue != 0) == (bool)right;
                    }
                    else
                    {
                        string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                        throw new Exception($"{positionInfo}Cannot compare boolean with non-numeric type: '{left}' == '{right}'");
                    }
                }
                else
                {
                    string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                    throw new Exception($"{positionInfo}Cannot compare values of different types: '{left}' == '{right}'");
                }
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object NotEqual(object left, object right, IToken token = null)
        {
            try
            {
                // We can reuse the Equal method and negate its result
                return !(bool)Equal(left, right, token);
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object GreaterThan(object left, object right, IToken token = null)
        {
            try
            {
                if ((left is int || left is float) && (right is int || right is float))
                {
                    return Convert.ToDouble(left) > Convert.ToDouble(right);
                }
                else if (left is string && right is string)
                {
                    return string.Compare((string)left, (string)right) > 0;
                }
                else
                {
                    string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                    throw new Exception($"{positionInfo}Cannot use '>' operator with types: '{left?.GetType()}' and '{right?.GetType()}'");
                }
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object LessThan(object left, object right, IToken token = null)
        {
            try
            {
                if ((left is int || left is float) && (right is int || right is float))
                {
                    return Convert.ToDouble(left) < Convert.ToDouble(right);
                }
                else if (left is string && right is string)
                {
                    return string.Compare((string)left, (string)right) < 0;
                }
                else
                {
                    string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                    throw new Exception($"{positionInfo}Cannot use '<' operator with types: '{left?.GetType()}' and '{right?.GetType()}'");
                }
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object GreaterThanOrEqual(object left, object right, IToken token = null)
        {
            try
            {
                if ((left is int || left is float) && (right is int || right is float))
                {
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);
                }
                else if (left is string && right is string)
                {
                    return string.Compare((string)left, (string)right) >= 0;
                }
                else
                {
                    string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                    throw new Exception($"{positionInfo}Cannot use '>=' operator with types: '{left?.GetType()}' and '{right?.GetType()}'");
                }
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object LessThanOrEqual(object left, object right, IToken token = null)
        {
            try
            {
                if ((left is int || left is float) && (right is int || right is float))
                {
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);
                }
                else if (left is string && right is string)
                {
                    return string.Compare((string)left, (string)right) <= 0;
                }
                else
                {
                    string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                    throw new Exception($"{positionInfo}Cannot use '<=' operator with types: '{left?.GetType()}' and '{right?.GetType()}'");
                }
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }
        // Logical operations
        public object LogicalAnd(object left, object right, IToken token = null)
        {
            try
            {
                bool leftBool = ConvertToBool(left, token);
                bool rightBool = ConvertToBool(right, token);

                return leftBool && rightBool;
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object LogicalOr(object left, object right, IToken token = null)
        {
            try
            {
                bool leftBool = ConvertToBool(left, token);
                bool rightBool = ConvertToBool(right, token);

                return leftBool || rightBool;
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object LogicalNot(object value, IToken token = null)
        {
            try
            {
                bool boolValue = ConvertToBool(value, token);
                return !boolValue;
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        // Helper method to convert any value to a boolean
        private bool ConvertToBool(object value, IToken token = null)
        {
            if (value is bool boolValue)
                return boolValue;

            if (value is int intValue)
                return intValue != 0;

            if (value is float floatValue)
                return floatValue != 0;

            if (value is string stringValue)
                return !string.IsNullOrEmpty(stringValue);

            string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
            throw new Exception($"{positionInfo}Cannot convert value '{value}' to boolean for logical operation.");
        }
        public object UnaryMinus(object value, IToken token = null)
        {
            try
            {
                if (value is int intValue)
                    return -intValue;

                if (value is float floatValue)
                    return -floatValue;

                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Cannot apply unary minus to non-numeric type: '{value}'");
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw;
            }
        }

        public object StringConcat(object left, object right, IToken token = null)
        {
            try
            {
                // Convert both operands to strings and concatenate
                return left.ToString() + right.ToString();
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                string positionInfo = token != null ? $"{token.Line}:{token.Column} - " : "";
                throw new Exception($"{positionInfo}Error during string concatenation: {ex.Message}");
            }
        }
    }
}
