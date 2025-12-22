declare global {
    const watari: {
        invoke<T>(method: string, ...args: any[]): Promise<T>;
        on(event: string, handler: (data: any) => void): void;
        off(event: string, handler: (data: any) => void): void;
        drop_zone(element: Element, callback: (paths: string[]) => void): () => void;
    };
}

export {};