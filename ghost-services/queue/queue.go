package queue

type Item struct {
	x, y int
	next *Item
}

type Queue struct {
	head *Item
	tail *Item
}

func New() *Queue {
	return &Queue{}
}

func (q *Queue) Enqueue(x, y int) {
	item := &Item{x: x, y: y}
	if q.tail == nil {
		q.head = item
		q.tail = item
	} else {
		q.tail.next = item
		q.tail = item
	}
}

func (q *Queue) Dequeue() (int, int) {
	if q.head == nil {
		return -1, -1
	}

	item := q.head
	q.head = item.next

	if q.head == nil {
		q.tail = nil
	}

	return item.x, item.y
}

func (q *Queue) IsEmpty() bool {
	return q.head == nil
}
