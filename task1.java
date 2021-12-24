package subtask1.atomicTypeTransactions;

import java.util.Arrays;
import java.util.concurrent.atomic.AtomicInteger;

class Bank {
    public static final int TEST_COUNT = 10000;
    private final AtomicInteger[] accounts;
    private long transactionCount = 0;

    public Bank(int accountCount, int initialBalance) {
        accounts = new AtomicInteger[accountCount];
        for (int i = 0; i < accounts.length; i++) {
            accounts[i] = new AtomicInteger(initialBalance);
        }
        transactionCount = 0;
    }

    public void transfer(int from, int to, int amount) throws InterruptedException {
        accounts[from].addAndGet(-amount);
        accounts[to].addAndGet(amount);
        transactionCount++;
        if (transactionCount % TEST_COUNT == 0) {
            test();
        }
    }
    public void test() {
        int sum = 0;
        for (AtomicInteger account : accounts) {
            sum += account.get();
        }
        System.out.println("Transactions:" + transactionCount + " Sum: " + sum);
    }

    public int size() {
        return accounts.length;
    }
}


public class BankTest {
    public static final int ACCOUNT_COUNT = 10;
    public static final int INITIAL_BALANCE = 10000;

    public static void main(String[] args) {
        Bank bank = new Bank(ACCOUNT_COUNT, INITIAL_BALANCE);
        int i;
        for (i = 0; i < ACCOUNT_COUNT; i++) {
            TransferThread transferThread = new TransferThread(bank, i, INITIAL_BALANCE);
            transferThread.setPriority(Thread.NORM_PRIORITY + i % 2);
            transferThread.start();
        }
    }
}


class TransferThread extends Thread {
    private final Bank bank;
    private final int fromAccount;
    private final int maxAmount;
    private static final int REPETITIONS = 10000;

    public TransferThread(Bank bank, int from, int max) {
        this.bank = bank;
        fromAccount = from;
        maxAmount = max;
    }

    public void run() {
        try {
            while (!interrupted()) {
                for (int i = 0; i < REPETITIONS; i++) {
                    int toAccount = (int)(bank.size() * Math.random());
                    int amount = (int)(maxAmount * Math.random() / REPETITIONS);
                    bank.transfer(fromAccount, toAccount, amount);
                    Thread.sleep(1);
                }
            }
        } catch (InterruptedException ignored) {}
    }
}
