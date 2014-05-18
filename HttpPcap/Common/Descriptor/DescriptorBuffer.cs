using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.Common
{
    class DescriptorBuffer : Descriptor
    {
        public static DescriptorBuffer create(int capacity)
        {
            if( capacity <= 0 )
            {
                throw new DescriptorException("DescriptorBuffer cannot create by negtive capacity.");
            }
	        DescriptorBuffer self = new DescriptorBuffer();
            self.des = new byte[capacity];
            self.desLength = 0;
            self.desCapacity = capacity;
            return self;
        }

	    public static DescriptorBuffer create(byte[] des , int index , int length , int capacity)
        {
            if(des == null)
            {
                throw new DescriptorException("DescriptorBuffer cannot create from null des.");
            }
            if(length > capacity)
            {
                throw new DescriptorException("DescriptorBuffer cannot create while length > capacity.");
            }
	        DescriptorBuffer self = DescriptorBuffer.create(capacity);
            self.copy(0, des, index, length);
	        return self;
        }

	    public static DescriptorBuffer create(byte[] des , int length)
        {
            return DescriptorBuffer.create(des, 0, length, length);
        }

	    public static DescriptorBuffer create(Descriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new DescriptorException("DescriptorBuffer cannot create from null descriptor.");
            }
	        else
            {
                return DescriptorBuffer.create(descriptor.des, 0, descriptor.desLength, descriptor.desCapacity);
            }
                
        }

        public void recapacity(int newCapacity, bool keepOriginalDes)
        {
	        if(newCapacity > desCapacity)
	        {
		        byte[] newDes = new byte[newCapacity];
		        if(keepOriginalDes)
                {
                    Array.Copy(this.des, newDes, this.desLength);
                }
                this.des = newDes;
                this.desCapacity = newCapacity;
	        }
        }

        public void abdicate( out byte[] desHeritor , out int desLength , out int desCapacity )
        {
            desHeritor = this.des;
            desLength = this.desLength;
            desCapacity = this.desCapacity;
            this.des = null;
            this.desLength = 0;
            this.desCapacity = 0;
        }
    }
}
