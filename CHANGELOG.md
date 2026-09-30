# Changelog

## [1.2.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v1.1.0...v1.2.0) (2026-09-30)


### Added

* **database:** migrate and seed development data ([64a6992](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/64a6992c07c1f7510c39810263a12a688d304c7b))
* **database:** migrate and seed development data ([7c77c8b](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/7c77c8bec35cb6dba37568f0c3ba38297bcc6314))
* **grpc:** propagate correlation IDs ([ff71dde](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/ff71ddebda9e7b6db5b0e72a3142e56a216987cd))
* **grpc:** propagate correlation IDs ([d31b1af](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d31b1af279eff3836148f0ed3f54882094b045b3))
* **hero:** add hero creation endpoint ([95b6af3](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/95b6af31c5ce904cfb713c504dd5f7c9d5a6baad))
* **hero:** add hero sheet endpoint ([07b8027](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/07b802745657d1ab5e41590cdf20392c014e3924))
* **hero:** expose class descriptions ([83a199d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/83a199d0c870ba6a4b884431c3205332367bdb24))
* **hero:** expose creation date in hero summaries ([65caf7a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/65caf7a288f9e15c736da58e795e4f2fd631320f))
* **hero:** expose creation date in hero summaries ([9e81fd8](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/9e81fd8c3797902550af8aa0a5be19fca24398fc))
* **hero:** list player heroes ([d1481ad](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d1481ad14419ebfb7b079c824c6a8d00f47878a1))
* **hero:** persist selected hero ([1844d96](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/1844d96ba5740a1bb5dd5ada58b245d0346aad06))
* **openapi:** persist user ID in Scalar ([9ec7b32](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/9ec7b3256facc8f14894a1c4f32cef252c3a78f8))
* **openapi:** persist user ID in Scalar ([f837244](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/f837244c80f77f5e8b929a72fe761a834094fc0c))
* **player:** add hero deselection endpoint ([43b2f8d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/43b2f8d9e6cc8651b3a5821b7d1e1168af167f68))
* **player:** add hero deselection endpoint ([831021a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/831021a3f6c2e49fdfb13923c2058452b6eb2fb7))
* **player:** implement player data model persistence ([94062d8](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/94062d8b3558a5245c361ffe3b46407582f9662b))
* **player:** implement player data model persistence ([89244c7](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/89244c7cf6764fae54f99eca78a05e0be9991788))
* **session:** start solo dungeon runs ([de16d5a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/de16d5abfdd6764daffc86484f1ad21d14bcbfb7))
* **template:** complete database and integration examples ([a3ed989](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/a3ed989b90febd9633a06bcddb733393524ba06d))


### Fixed

* **ci:** harden Docker publishing workflow ([d30f774](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d30f7749bd2ebda2a199d3390b76fa89e52e075f))
* **ci:** provision PostgreSQL for Sonar tests ([125c52a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/125c52ae5fd0a7e76ff4ada9089b6a19d3ab1b50))
* **database:** add Player initial migration ([3aae4fc](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/3aae4fc2f1afd5c0a76baa653c1ecc1d9d8dcb85))
* **database:** align migration workflow documentation ([1377c11](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/1377c112b8c1e3078eac0966d118789c404c5bb1))
* **database:** retain migration workflow ([791644c](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/791644c7e7669605c7c753b87d1816502d6568e5))
* **docker:** include dockerignore in application project ([fdedfc4](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/fdedfc42df92463c9fed47d76c222c3f11911dde))
* **docker:** include dockerignore in application project ([6014816](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/60148164bcc39252c68ae46fc06e1f96d68d9e84))
* **hero:** complete list summary merge ([930ed1f](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/930ed1f5cdee55a2e1bb39b78a6f754877e73ded))
* **hero:** isolate class listing integration test ([08c8c52](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/08c8c527969a9199aea7bcd813712f775da55a31))

## [1.1.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v1.0.0...v1.1.0) (2026-09-23)


### Added

* add ef core migration tooling ([b3abca4](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/b3abca46187ee86de0eda40e2da4ef26ef68e275))
* add ef core migration tooling ([e62b3f1](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/e62b3f12e77dce1622db927db6ad55cceaf05166))
* enforce command transaction boundary ([4096387](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/40963875235dde96cbfbe0d22352baf86f1d5255))
* publish player created events through RabbitMQ ([40727b6](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/40727b6125a422e1cbbeefaab2d17520c4ce2650))
* publish player created events through RabbitMQ ([cccf22e](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/cccf22eebfad251aee42150c4ba83fea8879c20c))

## [1.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v0.1.1...v1.0.0) (2026-09-22)


### ⚠ BREAKING CHANGES

* **contracts:** release protobuf contracts independently

### Added

* **contracts:** publish protobuf package ([5df7049](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/5df7049dcb876f7f821bc58759f0fef43d7e6ec6))
* **contracts:** release protobuf contracts independently ([d7cefcb](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d7cefcb8b517075887b9f33c90c4262bac64a212))
* **grpc:** add infrastructure player client ([00a2531](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/00a2531a16d93773e7bed45485eefc608314030a))
* **grpc:** expose player service internally ([b374c80](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/b374c80622924deaa333dabc33f350175f8b1e88))
* Mise en place d'un exemple de service GRPC client et serveur ([550c759](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/550c759e0f5de9d7d8cebb07b2f489016918cb07))

## [0.1.1](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v0.1.0...v0.1.1) (2026-09-05)


### Fixed

* run the container as the unprivileged app user ([#8](https://github.com/ProjectLanternDI3P1P2/player-backend/issues/8)) ([894be3f](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/894be3fe28e64218726b2dbc2e2bccb7efb9846b))
