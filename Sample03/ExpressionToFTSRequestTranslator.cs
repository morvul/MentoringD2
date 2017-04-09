using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sample03
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        string _constant;
        string _member;
        bool _isStartWithFilter;
        bool _isEndWithFilter;
        List<string> _requestParts;

        public List<string> Translate(Expression exp)
        {
            _constant = "";
            _member = "";
            _isStartWithFilter = false;
            _isEndWithFilter = false;
            _requestParts = new List<string>();
            
            Visit(exp);

            return _requestParts;
        }

        private string GetRequest()
        {
            string request = "";
            if (_member.Length > 0)
            {
                request = $"{_member}:({(_isEndWithFilter ? "*" : "")}{_constant}{(_isStartWithFilter ? "*" : "")})";
            }

            _isEndWithFilter = false;
            _isStartWithFilter = false;
            _member = "";
            _constant = "";

            return request;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);
                AddRequestPart();
                return node;
            }

            if (node.Method.DeclaringType == typeof(string)
                && node.Method.Name == "StartsWith")
            {
                base.VisitMethodCall(node);
                _isStartWithFilter = true;
                return node;
            }

            if (node.Method.DeclaringType == typeof(string)
                && node.Method.Name == "Contains")
            {
                base.VisitMethodCall(node);
                _isStartWithFilter = true;
                _isEndWithFilter = true;
                return node;
            }

            if (node.Method.DeclaringType == typeof(string)
                && node.Method.Name == "EndsWith")
            {
                base.VisitMethodCall(node);
                _isEndWithFilter = true;
                return node;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    if (!(node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                     && !(node.Right.NodeType == ExpressionType.MemberAccess && node.Left.NodeType == ExpressionType.Constant))
                        throw new NotSupportedException("Expression should contain a constant and property or field");
                    Visit(node.Left);
                    Visit(node.Right);
                    break;

                case ExpressionType.AndAlso:
                    Visit(node.Left);
                    AddRequestPart();
                    
                    Visit(node.Right);
                    AddRequestPart();

                    break;

                default:
                    throw new NotSupportedException(string.Format("Operation {0} is not supported", node.NodeType));
            }

            return node;
        }
        private void AddRequestPart()
        {
            var requestPart = GetRequest();
            if (requestPart.Length > 0)
            {
                _requestParts.Add(requestPart);
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _member = node.Member.Name;
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _constant = node.Value.ToString();
            return node;
        }
    }
}
