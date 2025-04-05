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

        public void DeclareVariable(string name, DataType type)
        {
            if (variables.ContainsKey(name))
            {
                throw new Exception($"Variable '{name}' is already declared.");
            }

            variableTypes[name] = type;
            variables[name] = type == DataType.Int ? 0 : (type == DataType.Float ? 0.0f : "");
        }

        // Získání hodnoty proměnné
        public object GetVariable(string name)
        {
            if (!variables.ContainsKey(name))
            {
                throw new Exception($"Variable '{name}' is not declared.");
            }

            return variables[name];
        }

        // Získání typu proměnné
        public DataType GetVariableType(string name)
        {
            if (!variableTypes.ContainsKey(name))
            {
                throw new Exception($"Variable '{name}' is not declared.");
            }

            return variableTypes[name];
        }

        // Přiřazení hodnoty proměnné s typovou kontrolou
        public object AssignVariable(string name, object value)
        {
            if (!variables.ContainsKey(name))
            {
                throw new Exception($"Variable '{name}' is not declared.");
            }

            DataType targetType = variableTypes[name];
            value = ConvertValueToType(value, targetType);
            variables[name] = value;
            return value;
        }

        public object ConvertValueToType(object value, DataType targetType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (targetType)
            {
                case DataType.Int:
                    if (value is float floatValue)
                        return (int)floatValue;
                    return Convert.ToInt32(value);

                case DataType.Float:
                    if (value is int intValue)
                        return (float)intValue;
                    return Convert.ToSingle(value);

                case DataType.String:
                    return value.ToString();

                default:
                    throw new ArgumentException($"Unsupported target type: {targetType}");
            }
        }

        // Aritmetické operace s typovou kontrolou
        public object Add(object left, object right)
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

        public object Subtract(object left, object right)
        {
            if (left is float || right is float)
            {
                return Convert.ToSingle(left) - Convert.ToSingle(right);
            }

            return Convert.ToInt32(left) - Convert.ToInt32(right);
        }

        public object Multiply(object left, object right)
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

        public object Divide(object left, object right)
        {
            if (IsZero(right))
                throw new DivideByZeroException("Division by zero");

            if (left is float || right is float)
            {
                return Convert.ToSingle(left) / Convert.ToSingle(right);
            }

            return Convert.ToInt32(left) / Convert.ToInt32(right);
        }

        public object Modulo(object left, object right)
        {
            if (left is float || right is float)
                throw new Exception("Modulo operator can only be used with integers");

            if (IsZero(right))
                throw new DivideByZeroException("Modulo by zero");

            return Convert.ToInt32(left) % Convert.ToInt32(right);
        }

        private bool IsZero(object value)
        {
            if (value is int intValue)
                return intValue == 0;
            if (value is float floatValue)
                return floatValue == 0;
            return false;
        }

        public DataType DetermineExpressionType(object value)
        {
            if (value is int)
                return DataType.Int;
            if (value is float)
                return DataType.Float;
            if (value is string)
                return DataType.String;

            throw new ArgumentException($"Unsupported value type: {value.GetType()}");
        }
    }
}
