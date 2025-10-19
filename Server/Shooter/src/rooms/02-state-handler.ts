import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";

export class Player extends Schema {
    @type("number")
    x = Math.floor(Math.random() * 50) - 25;

    @type("number")
    y = Math.floor(Math.random() * 50) - 25;

    @type("boolean")
    moving = false;
}

export class State extends Schema {
    @type({ map: Player })
    players = new MapSchema<Player>();

    createPlayer(sessionId: string) {
        this.players.set(sessionId, new Player());
    }

    removePlayer(sessionId: string) {
        this.players.delete(sessionId);
    }

    movePlayer (sessionId: string, position: any) {
    const player = this.players.get(sessionId);
    if (!player) return;

    if (position.x !== undefined) {
        player.x = position.x;
    }
    if (position.y !== undefined) {
        player.y = position.y;
    }
    if (position.moving !== undefined) {
        player.moving = position.moving; // Обновляем состояние движения
    }
}
}

export class StateHandlerRoom extends Room<State> {
    maxClients = 4;
    private fixedTimeStep: number = 1000 / 60; // 60 FPS для физики
    private accumulatedTime: number = 0;
    private lastTime: number = 0;

    onCreate(options: any) {
        console.log("StateHandlerRoom created!", options);

        this.setState(new State());

        // НАСТРОЙКИ FPS СЕРВЕРА:
        
        // 1. Частота симуляции игровой логики (60 FPS)
        this.setSimulationInterval((deltaTime) => this.update(deltaTime), this.fixedTimeStep);
        
        // 2. Частота отправки обновлений клиентам (20 FPS)
        this.setPatchRate(1000 / 20); // 50ms интервал
        
        // 3. Настройка задержки для компенсации пинга
        this.setSeatReservationTime(10); // секунды

        this.onMessage("move", (client, data) => {
            this.state.movePlayer(client.sessionId, data);
            
            // Логируем для отладки
            console.log(`Player ${client.sessionId} moved to:`, data);
        });

        this.onMessage("ping", (client) => {
            client.send("pong", Date.now());
        });

        this.lastTime = Date.now();
    }

    update(deltaTime: number) {
        // Фиксированный шаг времени для стабильной физики
        const currentTime = Date.now();
        const frameTime = currentTime - this.lastTime;
        this.lastTime = currentTime;

        this.accumulatedTime += frameTime;

        while (this.accumulatedTime >= this.fixedTimeStep) {
            this.fixedUpdate(this.fixedTimeStep);
            this.accumulatedTime -= this.fixedTimeStep;
        }

        // Дополнительная логика если нужно
    }

    fixedUpdate(deltaTime: number) {
        // deltaTime в миллисекундах
        const deltaSeconds = deltaTime / 1000;
        
        // Здесь можно обновлять физику или другую логику, требующую фиксированного шага
        // Например: столкновения, физическое движение и т.д.
        
        // Пример: обновление позиций на основе физики (если нужно)
        // this.state.players.forEach((player, sessionId) => {
        //     if (player.moving) {
        //         // Какая-то физическая логика
        //     }
        // });
    }

    onAuth(client: Client, options: any, request: any) {
        return true;
    }

    onJoin(client: Client, options: any, auth: any) {
        console.log("Client joined:", client.sessionId);
        this.state.createPlayer(client.sessionId);
        
        // Отправляем приветственное сообщение
        client.send("welcome", { 
            sessionId: client.sessionId,
            serverTime: Date.now(),
            settings: {
                patchRate: this.patchRate,
                simulationInterval: this.fixedTimeStep
            }
        });
    }

    onLeave(client: Client, consented: boolean) {
        console.log("Client left:", client.sessionId);
        this.state.removePlayer(client.sessionId);
    }

    onDispose() {
        console.log("Dispose StateHandlerRoom");
    }
}