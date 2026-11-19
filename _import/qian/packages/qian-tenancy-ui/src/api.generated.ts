// 由 OpenAPI 契约生成；运行对应包的 contracts:generate。
export interface paths {
    "/api/tenancy/tenants": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hf958b36220b04fdafafd4ce1bf1cc7bb286010b9174b9603f521db51c2838ed4"];
        put?: never;
        post: operations["H32e213ade311e9ee4ad412d3b841e07f7eb4e5e83c4a8b4f27b75763a085a00e"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H7282d39a5a69d6f1f24ed30a899bdd068a7ebf21ccdbff30f76bf9f0c76338a4"];
        put: operations["H0c94440aa57ef71f7214584848f8d938319335c6bc11651d69daea25551ccefb"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}:activate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["Hfb5635e6cfff70e8a4c5e59b16dd6743501530f0b6c26b47a6b08fa844dc414b"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}:suspend": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["Hd9c36e685d5005ed061faf9b8c30a0874c23b55e72d1465dc922c2f84fc8d7c4"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}:resume": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H4a21ef0f599db0f5482691b331fb045074833c31fe972f1e6d5a222803e665f5"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/meta/state-machine": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H0baeea7de8416226f77a77b251f92c9889405ba0e854f1a75fbf5aaa3d605c95"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/settings/{plugin}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H6e4552e1952535ae2b6d11e52db4e4104a4f85170191ae9715d76c2577f5474a"];
        put: operations["H14c3651c8980a91cd510dc051dcc0866c8a3a316587cbc581ccecbb0e34f56f8"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/quota": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hb102428e0a5eb1cef582378078f5c7bc09c86bfa196cb477a9d4478538804837"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/admin-accounts:lookup": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H8ecda9a6b94f4dcf0b3ba3123d1744496aa689c21706099504d4933e7c93d8b7"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/admin-accounts": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H2de9f42fa23f4a129a730895f52190bdee4fccb3123c68063553938cd1b4a48d"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/admins": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hf6d90ba241fbb8aad901e2660704d4710a1d2af4fd9601f2c4c35a899c8fe1e8"];
        put: operations["Hb4391e639d6f2aecd6af5b7150e5e8b15e77103b0895c71d03742630e6f9248d"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/admins/{membershipId}:reset-password": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H0d50c6458dc0456a17b1778ac4e2fdfec0daf83542a0f7d3420682f66f73fdde"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/summary": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H0dad2d4f1c2a907936436caac9a9aa704088f11b213a0b40148bcec73440aeee"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        H98ee36e693dce8c0534fa5498aa174271884d6e3c95afffed9488a0902088eff: string | number;
        H0c00afa8f99e817a162168bc2e67eff70c84db8ee09bc5766b13db3c85247d53: string;
        Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329: {
            type: string;
            title: string;
            status: string | number;
            detail?: string | null;
            instance?: string | null;
            code: string;
            message: string;
            trace_id: string;
            pointer?: string | null;
            allowed?: unknown[] | null;
            errors?: {
                code: string;
                message: string;
                pointer: string;
                allowed?: unknown[] | null;
            }[] | null;
        };
        H348e6230699972aceaa3dffa78181b0802bfd4f15ab880e0ed1a85f09f44ddd9: {
            items: {
                /** Format: uuid */
                id: string;
                code: string;
                name: string;
                /** @enum {unknown} */
                isolation: "row" | "schema" | "database";
                /** @enum {unknown} */
                status: "pending" | "trial" | "active" | "grace" | "suspended";
                revision?: string | number;
                /** Format: date-time */
                createdAt?: string;
                /** Format: date-time */
                validUntil?: string | null;
                usersLimit?: string | number | null;
                usersUsed?: string | number | null;
                administratorName?: string | null;
            }[];
            total?: string | number;
            counts?: {
                all: string | number;
                pending: string | number;
                trial: string | number;
                active: string | number;
                grace: string | number;
                suspended: string | number;
            };
        };
        H8d60b7a79fb017ad844f6928c4887390e07c978a83c08d2c4f9e9bd8f800ec83: {
            code: string;
            name: string;
            /**
             * @default row
             * @enum {unknown}
             */
            isolation: "row";
            administrator: {
                /** Format: uuid */
                membershipId: string;
                subjectId: string;
                initialPassword?: string | null;
            };
            quotas?: {
                /** @default null */
                users: string | number | null;
            } | null;
        };
        H1efbc1730f66df1de476f28ce486ec3cb73f2f1df786ce4ebaa42256f744e5ed: {
            /** Format: uuid */
            id: string;
            code: string;
            name: string;
            /** @enum {unknown} */
            isolation: "row" | "schema" | "database";
            /** @enum {unknown} */
            status: "pending" | "trial" | "active" | "grace" | "suspended";
            revision?: string | number;
        };
        /** Format: uuid */
        H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b: string;
        H7930ede332173eaa700236381f069a429477ff2d56dc436f3fb478f4bc63c77c: {
            name: string;
            expectedRevision: string | number;
            quotas?: {
                /** @default null */
                users: string | number | null;
            } | null;
        };
        H3c8bec34c5276d339fb827dae347ae8bac73ec583393e3b3cc43417c2b9b9dc2: {
            /** @default false */
            trial: boolean;
            /**
             * Format: date-time
             * @default null
             */
            validUntil: string | null;
        };
        /** 租户状态机元数据 */
        H591ab0ca13d5f0bee30459b9d177be3b8ada88c3bfb037965f829113e19a8b4d: {
            states: ("pending" | "trial" | "active" | "grace" | "suspended")[];
            transitions: {
                from: ("pending" | "trial" | "active" | "grace" | "suspended" | "any-s1" | "previous") | ("pending" | "trial" | "active" | "grace" | "suspended")[];
                action: string;
                to: ("pending" | "trial" | "active" | "grace" | "suspended" | "any-s1" | "previous") | ("pending" | "trial" | "active" | "grace" | "suspended")[];
            }[];
            actions: {
                name: string;
                /** @enum {unknown} */
                sideEffect: "read" | "write" | "destructive";
                agent: {
                    expose: boolean;
                    requiresConfirmation: boolean;
                };
                available?: boolean;
                stage?: string;
                idempotent?: boolean;
            }[];
        };
        H3d4543744a12cf3390ed55db1cc27bb586c02ebfbefae5fb63de698b7d5f09c1: {
            pluginId: string;
            entryId: string;
            revision: string | number;
            config: unknown;
        };
        H95e7bb1f8f8b01bfeabd6ef1d9e55302d53c996a31146d595f720df376aa8a40: {
            config: unknown;
            expectedRevision: string | number;
        };
        H5e4ab23a4c42eb2d5b42ab091d14c783d437966e4c5aca50d91b82366f317f16: {
            limits: {
                /** @default null */
                users: string | number | null;
            };
            usersUsed: string | number | null;
            usageAvailable: boolean;
        };
        H5702a1af94571b5d53869b1ca518fefb6800ee0096982c4f1a445f0df55e662d: {
            identifier: string;
        };
        H48cfd3243b7ed240ac294b585843127ae747c2765e051558934a42c09f8b1a29: {
            /** Format: uuid */
            subjectId: string;
            displayName: string;
            status: string;
            identifiers: {
                /** @enum {unknown} */
                kind: "username" | "phone" | "email";
                masked: string;
                verified: boolean;
            }[];
        };
        H58183670ef7618414622549e8c4f5e30d87fb38549ee5ce1a43778ab15cd9c82: {
            displayName: string;
            /** @enum {unknown} */
            identifierKind: "username" | "phone" | "email";
            identifier: string;
            initialPassword: string;
        };
        H1df9111f8b44f9c17e067b22b6460c8123dd58eed601095d64201275a368b2c8: {
            items: {
                /** Format: uuid */
                membershipId: string;
                /** Format: uuid */
                subjectId: string;
                displayName: string;
                account: string;
                status: string;
                identifiers: {
                    /** @enum {unknown} */
                    kind: "username" | "phone" | "email";
                    masked: string;
                    verified: boolean;
                }[];
            }[];
        };
        Hee650396fc7d1ff4066b85ef78878ae12066feecc2b5388d61bb2546c8871fad: {
            items: {
                /** Format: uuid */
                subjectId: string;
                initialPassword?: string | null;
            }[];
        };
        H16dbe4afdceef5979c0618c52965c5d046bfdefbd5e49dfab10ec15a7fa8c72a: {
            newPassword: string;
        };
        Hc3ce61efe197759416dd0d5c1dad32de3569453ac6f0963672dddffb83071d12: {
            /** Format: uuid */
            id: string;
            code: string;
            name: string;
            /** @enum {unknown} */
            status: "pending" | "trial" | "active" | "grace" | "suspended";
            /** Format: date-time */
            validUntil?: string | null;
            usersLimit?: string | number | null;
            usersUsed?: string | number | null;
            administratorName?: string | null;
        };
    };
    responses: never;
    parameters: never;
    requestBodies: never;
    headers: never;
    pathItems: never;
}
export type $defs = Record<string, never>;
export interface operations {
    Hf958b36220b04fdafafd4ce1bf1cc7bb286010b9174b9603f521db51c2838ed4: {
        parameters: {
            query?: {
                skip?: components["schemas"]["H98ee36e693dce8c0534fa5498aa174271884d6e3c95afffed9488a0902088eff"];
                take?: components["schemas"]["H98ee36e693dce8c0534fa5498aa174271884d6e3c95afffed9488a0902088eff"];
                status?: components["schemas"]["H0c00afa8f99e817a162168bc2e67eff70c84db8ee09bc5766b13db3c85247d53"];
                keyword?: components["schemas"]["H0c00afa8f99e817a162168bc2e67eff70c84db8ee09bc5766b13db3c85247d53"];
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H348e6230699972aceaa3dffa78181b0802bfd4f15ab880e0ed1a85f09f44ddd9"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H32e213ade311e9ee4ad412d3b841e07f7eb4e5e83c4a8b4f27b75763a085a00e: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H8d60b7a79fb017ad844f6928c4887390e07c978a83c08d2c4f9e9bd8f800ec83"];
            };
        };
        responses: {
            /** @description HTTP 201 */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1efbc1730f66df1de476f28ce486ec3cb73f2f1df786ce4ebaa42256f744e5ed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H7282d39a5a69d6f1f24ed30a899bdd068a7ebf21ccdbff30f76bf9f0c76338a4: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1efbc1730f66df1de476f28ce486ec3cb73f2f1df786ce4ebaa42256f744e5ed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H0c94440aa57ef71f7214584848f8d938319335c6bc11651d69daea25551ccefb: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H7930ede332173eaa700236381f069a429477ff2d56dc436f3fb478f4bc63c77c"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1efbc1730f66df1de476f28ce486ec3cb73f2f1df786ce4ebaa42256f744e5ed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    Hfb5635e6cfff70e8a4c5e59b16dd6743501530f0b6c26b47a6b08fa844dc414b: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H3c8bec34c5276d339fb827dae347ae8bac73ec583393e3b3cc43417c2b9b9dc2"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1efbc1730f66df1de476f28ce486ec3cb73f2f1df786ce4ebaa42256f744e5ed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    Hd9c36e685d5005ed061faf9b8c30a0874c23b55e72d1465dc922c2f84fc8d7c4: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1efbc1730f66df1de476f28ce486ec3cb73f2f1df786ce4ebaa42256f744e5ed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H4a21ef0f599db0f5482691b331fb045074833c31fe972f1e6d5a222803e665f5: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1efbc1730f66df1de476f28ce486ec3cb73f2f1df786ce4ebaa42256f744e5ed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H0baeea7de8416226f77a77b251f92c9889405ba0e854f1a75fbf5aaa3d605c95: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H591ab0ca13d5f0bee30459b9d177be3b8ada88c3bfb037965f829113e19a8b4d"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H6e4552e1952535ae2b6d11e52db4e4104a4f85170191ae9715d76c2577f5474a: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
                plugin: components["schemas"]["H0c00afa8f99e817a162168bc2e67eff70c84db8ee09bc5766b13db3c85247d53"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H3d4543744a12cf3390ed55db1cc27bb586c02ebfbefae5fb63de698b7d5f09c1"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H14c3651c8980a91cd510dc051dcc0866c8a3a316587cbc581ccecbb0e34f56f8: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
                plugin: components["schemas"]["H0c00afa8f99e817a162168bc2e67eff70c84db8ee09bc5766b13db3c85247d53"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H95e7bb1f8f8b01bfeabd6ef1d9e55302d53c996a31146d595f720df376aa8a40"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H3d4543744a12cf3390ed55db1cc27bb586c02ebfbefae5fb63de698b7d5f09c1"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    Hb102428e0a5eb1cef582378078f5c7bc09c86bfa196cb477a9d4478538804837: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H5e4ab23a4c42eb2d5b42ab091d14c783d437966e4c5aca50d91b82366f317f16"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H8ecda9a6b94f4dcf0b3ba3123d1744496aa689c21706099504d4933e7c93d8b7: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H5702a1af94571b5d53869b1ca518fefb6800ee0096982c4f1a445f0df55e662d"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H48cfd3243b7ed240ac294b585843127ae747c2765e051558934a42c09f8b1a29"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H2de9f42fa23f4a129a730895f52190bdee4fccb3123c68063553938cd1b4a48d: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H58183670ef7618414622549e8c4f5e30d87fb38549ee5ce1a43778ab15cd9c82"];
            };
        };
        responses: {
            /** @description HTTP 201 */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H48cfd3243b7ed240ac294b585843127ae747c2765e051558934a42c09f8b1a29"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    Hf6d90ba241fbb8aad901e2660704d4710a1d2af4fd9601f2c4c35a899c8fe1e8: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1df9111f8b44f9c17e067b22b6460c8123dd58eed601095d64201275a368b2c8"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    Hb4391e639d6f2aecd6af5b7150e5e8b15e77103b0895c71d03742630e6f9248d: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["Hee650396fc7d1ff4066b85ef78878ae12066feecc2b5388d61bb2546c8871fad"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H1df9111f8b44f9c17e067b22b6460c8123dd58eed601095d64201275a368b2c8"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H0d50c6458dc0456a17b1778ac4e2fdfec0daf83542a0f7d3420682f66f73fdde: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
                membershipId: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H16dbe4afdceef5979c0618c52965c5d046bfdefbd5e49dfab10ec15a7fa8c72a"];
            };
        };
        responses: {
            /** @description HTTP 204 */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
    H0dad2d4f1c2a907936436caac9a9aa704088f11b213a0b40148bcec73440aeee: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H875d18ad73a6647bb01b2262ae6f3d4f4e7320aaf1223e60ad82e25b12f80b6b"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hc3ce61efe197759416dd0d5c1dad32de3569453ac6f0963672dddffb83071d12"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Hd28d4e076a32fe1a981549c039ce980bcf139093826533199211f2488154a329"];
                };
            };
        };
    };
}
