using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
	public partial class ClientConnection
	{
		private RemoteServerConnection _Remote;
		public RemoteServerConnection SendToServer
		{
			get
			{
				if (_Remote == null)
					_Remote = new RemoteServerConnection(this);
				return _Remote;
			}
		}
		public void Receive_RecievePacket(NetworkReader reader)
		{
			Int32 packetNum = reader.ReadInt32();
			String contents = reader.ReadString();
			RecievePacket(packetNum, contents);
		}
		
		public void Receive_RecieveServerPerformanceDictionary(NetworkReader reader)
		{
			Int32 performanceType = reader.ReadInt32();
			Single performanceVal = reader.ReadSingle();
			RecieveServerPerformanceDictionary(performanceType, performanceVal);
		}
		
		public void Receive_ClientPrepareToRecieveTransmission(NetworkReader reader)
		{
			Int32 transmissionId = reader.ReadInt32();
			Int32 expectedSize = reader.ReadInt32();
			Vector3 centralCarPos = reader.ReadVector3();
			ClientPrepareToRecieveTransmission(transmissionId, expectedSize, centralCarPos);
		}
		
		public void Receive_ClientRecieveTransmission(NetworkReader reader)
		{
			Int32 transmissionId = reader.ReadInt32();
			Int32 recBuffer_count = reader.ReadInt32();
			System.Byte[] recBuffer = new System.Byte[recBuffer_count];
			for (int recBuffer_index = 0; recBuffer_index < recBuffer_count; recBuffer_index++)
			recBuffer[recBuffer_index] = reader.ReadByte();
			ClientRecieveTransmission(transmissionId, recBuffer);
		}
		
		public class RemoteServerConnection
		{
			private INetworkSender _netSender;
			public RemoteServerConnection(INetworkSender netSender)
			{
				_netSender = netSender;
			}
			
			public void RecieveObjectPosition(Int32 objectID, Vector3 position, Vector3 rotation, Vector3 localScale)
			{
				NetworkWriter writer = _netSender.CreateWriter(-1684256861);
				writer.Write(objectID);
				writer.Write(position);
				writer.Write(rotation);
				writer.Write(localScale);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void RecieveObjectState(Int32 objectID, Boolean active)
			{
				NetworkWriter writer = _netSender.CreateWriter(214054999);
				writer.Write(objectID);
				writer.Write(active);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void RecieveObjectStateAndPosition(Int32 objectID, Boolean active, Vector3 position, Vector3 rotation, Vector3 localScale)
			{
				NetworkWriter writer = _netSender.CreateWriter(542266185);
				writer.Write(objectID);
				writer.Write(active);
				writer.Write(position);
				writer.Write(rotation);
				writer.Write(localScale);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void RecievePacket(Int32 packetNum, String contents)
			{
				NetworkWriter writer = _netSender.CreateWriter(55045539);
				writer.Write(packetNum);
				writer.Write(contents);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void RecieveNewCarSpawn(Int32 objectID, Boolean active)
			{
				NetworkWriter writer = _netSender.CreateWriter(-1558999668);
				writer.Write(objectID);
				writer.Write(active);
				_netSender.PrepareAndSendWriter(writer);
			}
			
		}
	}
}
