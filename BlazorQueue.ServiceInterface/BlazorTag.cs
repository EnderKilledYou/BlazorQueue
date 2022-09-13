using System;
using System.Collections.Generic;


namespace BlazorQueue.ServiceInterface
{
    public class BlazorTag : IEquatable<BlazorTag>
    {
      
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as BlazorTag);
        }

        public bool Equals(BlazorTag other)
        {
            return other is not null &&
                   Name == other.Name;
        }

        public static bool operator ==(BlazorTag left, BlazorTag right)
        {
            return EqualityComparer<BlazorTag>.Default.Equals(left, right);
        }

        public static bool operator !=(BlazorTag left, BlazorTag right)
        {
            return !(left == right);
        }
    }
}
