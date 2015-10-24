// Author(s): Sébastien Lorion
// Original idea: Gregory Young on ADVANCED-DOTNET discussion list (http://discuss.develop.com/advanced-dotnet.html)

using System;
using System.Collections.Generic;

namespace NLight.Collections
{
	/// <summary>
	/// Represents a buffer manager.
	/// </summary>
	/// <remarks>
	/// This class maintains a set of large segments and gives clients pieces of these
	/// segments that they can use as buffers. An alternative to this would be to
	/// create many small arrays which then need to be maintained. In comparaison, 
	/// using this class should be slightly both easier and more efficient because by creating only 
	/// a few very large objects, it will force these objects to be placed on the Large Object Heap (LOH).
	/// Since the objects are on the LOH, they are not subject to compacting, an expensive operation
	/// which would require an update of all GC roots - as would be the case with lots of smaller arrays
	/// that are allocated in the normal heap.
	/// </remarks>
	public class BufferManager<T>
	{
		/// <summary>
		/// Contains the number of chunks to create per segment.
		/// </summary>
		private readonly int _chunkPerSegmentCount;

		/// <summary>
		/// Contains the size of each chunk.
		/// </summary>
		private readonly int _chunkSize;

		/// <summary>
		/// Contains the size of a segment (_chunkPerSegmentCount * _chunkSize).
		/// </summary>
		private readonly int _segmentSize;

		/// <summary>
		/// Contains the available buffers.
		/// </summary>
		private readonly Stack<ArraySegment<T>> _availableBuffers;

		/// <summary>
		/// Contains the allocated segments.
		/// </summary>
		private readonly List<T[]> _segments;

		/// <summary>
		/// Initializes a new instance of the BufferManager class.
		/// </summary>
		/// <param name="chunkPerSegmentCount">The number of chunks to create per segment.</param>
		/// <param name="chunkSize">The size of each chunk.</param>
		public BufferManager(int chunkPerSegmentCount, int chunkSize)
			: this(chunkPerSegmentCount, chunkSize, 1)
		{
		}

		/// <summary>
		/// Initializes a new instance of the BufferManager class.
		/// </summary>
		/// <param name="chunkPerSegmentCount">The number of chunks to create per segment.</param>
		/// <param name="chunkSize">The size of each chunk.</param>
		/// <param name="initialSegmentCount">The initial number of segments to create.</param>
		public BufferManager(int chunkPerSegmentCount, int chunkSize, int initialSegmentCount)
		{
			if (chunkPerSegmentCount < 1) throw new ArgumentOutOfRangeException(nameof(chunkPerSegmentCount));
			if (chunkSize < 1) throw new ArgumentOutOfRangeException(nameof(chunkSize));
			if (initialSegmentCount < 0) throw new ArgumentOutOfRangeException(nameof(initialSegmentCount));

			_chunkPerSegmentCount = chunkPerSegmentCount;
			_chunkSize = chunkSize;
			_segmentSize = _chunkPerSegmentCount * _chunkSize;

			_segments = new List<T[]>(Math.Max(4, initialSegmentCount));
			_availableBuffers = new Stack<ArraySegment<T>>(chunkPerSegmentCount * initialSegmentCount);

			for (int i = 0; i < initialSegmentCount; i++)
				CreateNewSegment();
		}

		/// <summary>
		/// Gets the current number of buffers available.
		/// </summary>
		public int AvailableBufferCount => _availableBuffers.Count;

		/// <summary>
		/// Gets the total size of all buffers.
		/// </summary>
		public int TotalBufferSize => _segments.Count * _segmentSize;

		//TODO: a TrimExcess() method or something similar to release memory when required

		/// <summary>
		/// Requests a buffer from the manager.
		/// </summary>
		/// <returns>A new uninitialized buffer.</returns>
		/// <remarks>It is the client's responsibility to release the buffer after usage.</remarks>
		public ArraySegment<T> Request()
		{
			lock (_availableBuffers)
			{
				if (_availableBuffers.Count == 0)
					CreateNewSegment();

				return _availableBuffers.Pop();
			}
		}

		/// <summary>
		/// Releases a buffer and returns it to the manager.
		/// </summary>
		/// <param name="buffer">The buffer to release.</param>
		/// <remarks>
		/// It is the client's responsibility to release the buffer after usage.
		/// </remarks>
		public void Release(ArraySegment<T> buffer)
		{
			lock (_availableBuffers)
			{
				_availableBuffers.Push(buffer);
			}
		}

		/// <summary>
		/// Creates a new segment and makes its associated chunks available as buffers.
		/// </summary>
		private void CreateNewSegment()
		{
			T[] segment = new T[_chunkPerSegmentCount * _chunkSize];
			_segments.Add(segment);

			for (int i = 0; i < _chunkPerSegmentCount; i++)
			{
				ArraySegment<T> chunk = new ArraySegment<T>(segment, i * _chunkSize, _chunkSize);
				_availableBuffers.Push(chunk);
			}
		}
	}
}