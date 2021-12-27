package com.example.task1;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.atomic.AtomicReference;

public class CasMutex extends Object {
    private AtomicReference<Runnable> currThread = new AtomicReference<>();
    private LinkedBlockingQueue<Runnable> waitingThreads;

    public CasMutex(LinkedBlockingQueue<Runnable> waitingThreads) {
        this.waitingThreads = waitingThreads;
    }
    public void lock(){
        if (Thread.currentThread().equals(currThread.get())) {
            throw new RuntimeException("locked");
        }
        while (!currThread.compareAndSet(null, Thread.currentThread())) {
            Thread.yield();
        }
        System.out.println("locked by: " + Thread.currentThread().getName());
    }
    public void unlock() {
        if (!Thread.currentThread().equals(currThread.get())) {
            throw new RuntimeException("not locked");
        }
        System.out.println("unlocked by: " + Thread.currentThread().getName());
        currThread.set(null);
    }
    public void casWait() throws InterruptedException {
        Thread thread = Thread.currentThread();
        if (!thread.equals(currThread.get())) {
            throw new RuntimeException("not locked");
        }
        waitingThreads.put(thread);
        System.out.println("Wait: " + Thread.currentThread().getName());
        unlock();
        while (waitingThreads.contains(thread)) {
            Thread.yield();
        }
        lock();
        System.out.println("No wait: " + Thread.currentThread().getName());
    }
    public void casNotify() throws InterruptedException {
        waitingThreads.take();
        System.out.println("Notify: " + Thread.currentThread().getName());
    }
    public void casNotifyAll() {
        waitingThreads.clear();
        System.out.println("Notify all: " + Thread.currentThread().getName());
    }
}
