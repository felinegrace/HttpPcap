using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.Common
{
    class DescriptorReference : Descriptor
    {

        public DescriptorReference( byte[] des, int desLength , int desCapacity )
        {
	        this.des = des;
            this.desLength = desLength;
            this.desCapacity = desCapacity;
        }

        public DescriptorReference(byte[] des, int desLength) : this(des, desLength, desLength)
        {
        }

        public DescriptorReference(Descriptor descriptor)
            : this(descriptor.des, descriptor.desLength, descriptor.desLength)
        {
        }

    }
}
