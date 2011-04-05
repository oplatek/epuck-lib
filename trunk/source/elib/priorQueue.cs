using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elib {
  public partial class Sercom {
    
    sealed class node { 
      public node(ansGuard elem_) {
        elem = elem_;
        prev = next = null;
      }
      public ansGuard Elem { get { return elem; } set { elem = value; } }
      public node Prev { get { return prev; } set { prev = value; } }
      public node Next { get { return next; } set { next = value; } }
      ansGuard elem;
      node next;
      node prev;
    }
    
    sealed class priorQueue : IEnumerable<ansGuard>{
      public event EventHandler Empty;
      public event EventHandler NonEmpty;
      node head;
      long count;
      node end;

      void OnEmpty(object Sender, EventArgs e) {
        if (Empty != null) 
          Empty(this, e);        
      }
      void OnNonEmpty(object Sender, EventArgs e) {
        if (NonEmpty!= null)
          NonEmpty(this, e);
      }

      public void Remove(node x) {
        if (count <= 0)
          throw new InvalidOperationException("myQueue is empty");
        count--;
        if (x.Prev == null) {
          head = x.Next;
        } else {
          x.Prev.Next = x.Next;
        }
        if (x.Next == null) {
          end = x.Prev;
        } else {
          x.Next.Prev = x.Prev;
        }
        if (count == 0)
          OnEmpty(this, null);
      }
      public priorQueue() { head = end = null; count = 0; Empty = null; }
      
      public node Enqueue(node p) {        
        p.Prev = null;
        p.Next = head;
        if (head != null)
          head.Prev = p;
        else {
          end = p;          
        }
        head = p;
        Count++;
        node res= sortInX(head);
        OnNonEmpty(this,null);
        return res;
      }
      node sortInX(node x) {
        while (x.Next != null && (x.Elem.CompareTo(x.Next.Elem) > 0)) {
          if (x.Prev == null)
            head = x.Next;
          else
            x.Prev.Next = x.Next;          
          x.Next.Prev = x.Prev;
          node xnn=x.Next.Next;
          x.Next.Next = x;
          x.Prev = x.Next;
          x.Next = xnn;
          if (xnn != null)
            xnn.Prev = x;
        }
        if (x.Next == null)
          end = x;
        return x;
      }
      
      public bool Dequeue() {
        if (count<=0) {
          return false;
        }
        head = head.Next;
        if(head!=null)
          head.Prev = null;
        count--;
        if (count == 0)
          OnEmpty(this, null);
        return true;
      }
      public node Peek { get { return head; } }
      public long Count { get { return count; } private set { count = value; } }
      public void Clear() {  
        head = end = null; 
        count = 0; 
      }
      #region IEnumarable
      sealed class myIt : IEnumerator<ansGuard>, IEnumerator  {
        node head;
        node reset;
        node current;
        bool disposed;
        public myIt(node head_) {
          head = head_;
          disposed = false;
          reset = new node(null);
          reset.Next = head;
          Reset();
        }
        public ansGuard Current { get { return current.Elem; } }
        public bool MoveNext() {
          if (current.Next != null) {
            current = current.Next;
            return true;
          } else
            return false;
        }
        public void Reset() { current = reset; }        
        object IEnumerator.Current { get { return this.Current; } }

        public void Dispose() {
          if (!disposed) {
            disposed = true;
            head = null;
            reset = null;
            current = null;
          }
        }
      }
      IEnumerator<ansGuard> IEnumerable<ansGuard>.GetEnumerator() {
        return new myIt(head);
      }
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return new myIt(head); }
      #endregion      
    }

    sealed class myQueue : IEnumerable<node> { 
      node head; 
      node end;
      int count;
      public event EventHandler Empty;
      public event EventHandler NonEmpty;

      public myQueue() {
        head = null;
        end = null;
        count = 0;
      }

      void OnEmpty(object Sender, EventArgs e) {
        if (Empty != null)
          Empty(this, e);
      }
      void OnNonEmpty(object Sender, EventHandler e) {
        if (NonEmpty != null)
          NonEmpty(null, null);
      }

      public void Remove(node x) {
        if(count<=0)
          throw new InvalidOperationException("myQueue is empty");
        count--;
        if (x.Prev == null) {
          head = x.Next;
        } else {
          x.Prev.Next = x.Next;
        }
        if (x.Next == null) {
          end = x.Prev;
        } else {
          x.Next.Prev = x.Prev;
        }
        if (count == 0)
          OnEmpty(this, null);
      }
      public int Count { get { return count; } }
      public node Enqueue(node x) {
        count++;        
        if (head == null) {
          head = end = x;
        } else {
          x.Next = head;
          head.Prev = x;
          head = x;                    
        }
        OnNonEmpty(this,null);
        return x;
      }
      public node Peek { get { return end; } }
      public bool Dequeue() {
        if (count <= 0)
          return false;
        end = end.Prev;
        if (end != null)
          end.Next = null;
        else
          head = null;
        count--;
        if (count == 0)
          OnEmpty(this, null);
        return true ;
      }
      public override string ToString() {
        StringBuilder ans=new StringBuilder("priorQueue<T> with ");
        ans.AppendFormat("head={0};end={1};count={2}",head,end,count);
        return ans.ToString();
      }
      public void Clear() {
        head = null;
        end = null;
        count = 0;
      }

      #region IEnumarable
      sealed class myIt : IEnumerator<node>, IEnumerator  {
        bool disposed;
        node head;
        node reset;
        node current;
        public myIt(node head_) {
          disposed = false;
          head = head_;
          reset = new node(null);
          reset.Next = head;
          Reset();
        }
        public node Current { get { return current; } }
        public bool MoveNext() {
          if (current.Next != null) {
            current = current.Next;
            return true;
          } else
            return false;
        }
        public void Reset() { current = reset; }       
        object IEnumerator.Current { get { return this.Current; } }

        public void Dispose(){
          if (!disposed) {
            disposed = true;
            head = null;
            reset = null;
            current = null;
          }
        }
      }

      IEnumerator<node> IEnumerable<node>.GetEnumerator() { return new myIt(head); }
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return new myIt(head); }
      #endregion
    }
  }
}
